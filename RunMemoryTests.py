import subprocess
import xml.etree.ElementTree as ET
import os
import sys
import argparse
import signal
import time
import threading
import psutil
import csv
import glob

# Configuration
TRX_FILE = os.path.abspath("memory_test_temp.trx")
CSV_FILE = os.path.abspath("memory_stats.csv")

# Global state for monitoring
monitor_active = False
current_pid = None
memory_samples = []

def signal_handler(sig, frame):
    print("\n\nStopping...")
    cleanup()
    sys.exit(0)

def cleanup():
    if os.path.exists(TRX_FILE):
        try:
            os.remove(TRX_FILE)
        except OSError:
            pass

def monitor_memory(pid, interval=0.1):
    global monitor_active, memory_samples
    try:
        process = psutil.Process(pid)
        while monitor_active:
            try:
                # Get RSS memory in MB
                mem = process.memory_info().rss / (1024 * 1024)
                memory_samples.append(mem)

                # Also check children? Dotnet test might spawn child processes (testhost)
                # For simplicity, we monitor the main process tree peak?
                # Usually testhost is a child.
                children = process.children(recursive=True)
                for child in children:
                    try:
                        mem += child.memory_info().rss / (1024 * 1024)
                    except (psutil.NoSuchProcess, psutil.AccessDenied):
                        pass

                # Correct sample to include children sum
                memory_samples[-1] = mem

            except (psutil.NoSuchProcess, psutil.AccessDenied):
                break
            time.sleep(interval)
    except psutil.NoSuchProcess:
        pass

def get_all_tests(project_path):
    print("Discovering tests...")
    cmd = ["dotnet", "test", project_path, "--list-tests"]
    result = subprocess.run(cmd, capture_output=True, text=True)

    tests = []
    capture = False
    for line in result.stdout.splitlines():
        if "The following Tests are available:" in line:
            capture = True
            continue
        if capture and line.strip():
            tests.append(line.strip())

    print(f"Found {len(tests)} tests.")
    return tests

def run_test_and_monitor(project_path, test_name):
    global monitor_active, memory_samples

    cmd = [
        "dotnet", "test", project_path,
        "--filter", f"FullyQualifiedName={test_name}",
        "--logger", f"trx;LogFileName={TRX_FILE}"
    ]

    memory_samples = []
    monitor_active = True

    # Start process
    proc = subprocess.Popen(cmd, stdout=subprocess.DEVNULL, stderr=subprocess.PIPE)

    # Start monitor thread
    monitor_thread = threading.Thread(target=monitor_memory, args=(proc.pid,))
    monitor_thread.start()

    # Wait for completion
    proc.wait()

    # Stop monitor
    monitor_active = False
    monitor_thread.join()

    # Parse result
    passed = False
    error_msg = ""
    if os.path.exists(TRX_FILE):
        passed, error_msg = parse_trx_result(TRX_FILE)
        os.remove(TRX_FILE)

    # Calculate stats
    if memory_samples:
        min_mem = min(memory_samples)
        max_mem = max(memory_samples)
        avg_mem = sum(memory_samples) / len(memory_samples)
    else:
        min_mem = max_mem = avg_mem = 0

    return {
        "Test": test_name,
        "Result": "Pass" if passed else "Fail",
        "MinMB": min_mem,
        "MaxMB": max_mem,
        "AvgMB": avg_mem,
        "Samples": len(memory_samples),
        "Error": error_msg
    }

def parse_trx_result(trx_path):
    try:
        tree = ET.parse(trx_path)
        root = tree.getroot()
        # Namespace handling is annoying in ET, ignore it by using endswith match in logic

        passed = True
        error_msg = ""

        for elem in root.iter():
            if elem.tag.endswith('UnitTestResult'):
                outcome = elem.get('outcome')
                if outcome != 'Passed':
                    passed = False
                    # Try to find error message
                    for output in elem.iter():
                        if output.tag.endswith('Message'):
                            error_msg = output.text
                            break
        return passed, error_msg
    except Exception as e:
        return False, str(e)

def main():
    signal.signal(signal.SIGINT, signal_handler)

    project_path = "ImageAutomate/ImageAutomate.Execution.MemoryAndAccessTests/ImageAutomate.Execution.MemoryAndAccessTests.csproj"

    tests = get_all_tests(project_path)

    results = []

    print(f"{'Test Name':<60} | {'Result':<6} | {'Max MB':<8} | {'Avg MB':<8}")
    print("-" * 90)

    for i, test in enumerate(tests):
        print(f"Running ({i+1}/{len(tests)}): {test[-50:]}...", end='\r')
        stats = run_test_and_monitor(project_path, test)
        results.append(stats)

        # Print row
        name_display = (test[:57] + '..') if len(test) > 57 else test
        print(f"{name_display:<60} | {stats['Result']:<6} | {stats['MaxMB']:<8.2f} | {stats['AvgMB']:<8.2f}")

    # Global Stats
    all_max = max([r['MaxMB'] for r in results]) if results else 0
    all_min = min([r['MinMB'] for r in results]) if results else 0
    all_avg = sum([r['AvgMB'] for r in results]) / len(results) if results else 0

    print("\n" + "=" * 90)
    print(f"Global Stats: Max={all_max:.2f}MB, Min={all_min:.2f}MB, Avg={all_avg:.2f}MB")

    # Save CSV
    with open(CSV_FILE, 'w', newline='') as csvfile:
        fieldnames = ['Test', 'Result', 'MinMB', 'MaxMB', 'AvgMB', 'Samples', 'Error']
        writer = csv.DictWriter(csvfile, fieldnames=fieldnames)

        writer.writeheader()
        for r in results:
            writer.writerow(r)

    print(f"Detailed results saved to {CSV_FILE}")

if __name__ == "__main__":
    main()
