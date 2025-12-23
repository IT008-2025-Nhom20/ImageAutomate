import subprocess
import xml.etree.ElementTree as ET
import os
import sys
import argparse
import signal
import shutil
import time

# Configuration
# Force absolute path to ensure we know exactly where the file is
TRX_FILE = os.path.abspath("stress_test_temp.trx")
FAIL_LOG_FILE = os.path.abspath("failed_tests.log")

# { "ClassName": { "TestName": { "pass": 0, "fail": 0 } } }
results = {}
run_count = 0
failed = False

def signal_handler(sig, frame):
    print("\n\nStopping... Generating report.")
    print_report()
    cleanup()
    sys.exit(0)

def cleanup():
    if os.path.exists(TRX_FILE):
        try:
            os.remove(TRX_FILE)
        except:
            pass

def print_report():
    print(f"\n--- Stress Test Report (Runs: {run_count}) ---")
    if not results:
        print("No results collected. (Did the build fail?)")
        return

    # Iterate over containers (Classes)
    sorted_containers = sorted(results.keys())

    for container in sorted_containers:
        print(f"\nContainer: {container}")
        print(f"{'Test Case':<80} | {'Pass':<6} | {'Fail':<6} | {'Fail %':<8}")
        print("-" * 110)

        container_results = results[container]
        
        # Sort by failure count descending
        sorted_tests = sorted(
            container_results.items(), 
            key=lambda item: (item[1]['fail'], item[0]), 
            reverse=True
        )

        for name, stats in sorted_tests:
            total = stats['pass'] + stats['fail']
            fail_rate = (stats['fail'] / total) * 100 if total > 0 else 0.0
            
            # Display full name, truncate if absolutely necessary but make it wide
            display_name = (name[:77] + '..') if len(name) > 77 else name
            row = f"{display_name:<80} | {stats['pass']:<6} | {stats['fail']:<6} | {fail_rate:.1f}%"
            
            if stats['fail'] > 0:
                print(f"\033[91m{row}\033[0m") 
            else:
                print(row)

def parse_trx(file_path, record_failures):
    global failed
    try:
        tree = ET.parse(file_path)
        root = tree.getroot()

        namespaces = {'ns': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
        
        # 1. Map testId to ClassName
        # Structure: TestRun -> TestDefinitions -> UnitTest -> TestMethod (className)
        test_id_map = {} # testId -> className

        # Handle optional namespace in find/findall
        # Helper to find with or without namespace if needed, but usually TRX has the namespace above.
        # We'll try to detect or just use ending logic like before for robustness.
        
        for elem in root.iter():
            if elem.tag.endswith('UnitTest'):
                test_id = elem.get('id')
                # Find TestMethod child
                class_name = "Unknown"
                for child in elem:
                    if child.tag.endswith('TestMethod'):
                        class_name = child.get('className')
                        break
                if test_id:
                    test_id_map[test_id] = class_name

        # 2. Parse Results
        for elem in root.iter():
            if elem.tag.endswith('UnitTestResult'):
                test_id = elem.get('testId')
                test_name = elem.get('testName')
                outcome = elem.get('outcome') # 'Passed', 'Failed'

                # Resolve Class Name
                class_name = test_id_map.get(test_id, "UnknownContainer")

                if class_name not in results:
                    results[class_name] = {}
                
                if test_name not in results[class_name]:
                    results[class_name][test_name] = {'pass': 0, 'fail': 0}

                if outcome == 'Passed':
                    results[class_name][test_name]['pass'] += 1
                elif outcome == 'Failed':
                    results[class_name][test_name]['fail'] += 1
                    failed = True
                    
                    if record_failures:
                        record_failure_details(elem, class_name, test_name)

    except ET.ParseError:
        print(f"[!] XML Parse Error on run {run_count}")

def record_failure_details(result_elem, class_name, test_name):
    # Extract error message and stack trace
    error_msg = ""
    stack_trace = ""
    
    for output in result_elem:
        if output.tag.endswith('Output'):
            for error_info in output:
                if error_info.tag.endswith('ErrorInfo'):
                    for child in error_info:
                        if child.tag.endswith('Message'):
                            error_msg = child.text
                        elif child.tag.endswith('StackTrace'):
                            stack_trace = child.text

    try:
        with open(FAIL_LOG_FILE, "a", encoding="utf-8") as f:
            f.write(f"--- Run {run_count} Failure ---\n")
            f.write(f"Test: {class_name}.{test_name}\n")
            f.write(f"Timestamp: {time.strftime('%Y-%m-%d %H:%M:%S')}\n")
            f.write(f"Message:\n{error_msg}\n")
            f.write(f"Stack Trace:\n{stack_trace}\n")
            f.write("-" * 50 + "\n\n")
    except Exception as e:
        print(f"[!] Failed to record failure details: {e}")

def main():
    global run_count, failed

    parser = argparse.ArgumentParser()
    parser.add_argument('--runs', type=int, default=10,
                        help='Number of times to run the tests (ignored if --run-until-fail is used)')
    parser.add_argument('--run-until-fail', action='store_true',
                        help='Run tests repeatedly until a failure occurs')
    parser.add_argument('--filter', type=str, default='',
                        help='Filter for tests (e.g., "FullyQualifiedName=MyTestMethod" or just "MyTestMethod")')
    parser.add_argument('--project', type=str, default='',
                        help='Path to the test project file (required when using --filter with just a method name)')
    parser.add_argument('-c', '--configuration', type=str, default='Debug',
                        help='Build configuration (Debug or Release)')
    parser.add_argument('--record-failed-results', action='store_true',
                        help='Record details of failed tests to failed_tests.log')

    args = parser.parse_args()

    signal.signal(signal.SIGINT, signal_handler)

    # Clear previous fail log if it exists and we are starting a new session
    # (Optional: user might want to append, but usually a fresh run implies fresh logs. 
    # Let's keep append behavior in loop, but maybe clear at start? 
    # I'll clear it at start for clarity of this specific execution session)
    if args.record_failed_results and os.path.exists(FAIL_LOG_FILE):
        try:
            os.remove(FAIL_LOG_FILE)
            print(f"Cleared previous failure log: {FAIL_LOG_FILE}")
        except:
            pass

    cmd = [
        "dotnet", "test",
        "--configuration", args.configuration,
        "--logger", f"trx;LogFileName={TRX_FILE}"
    ]

    if args.project:
        cmd.insert(2, args.project)

    if args.filter:
        if '=' not in args.filter:
            filter_value = f"FullyQualifiedName={args.filter}"
            cmd.extend(["--filter", filter_value])
        else:
            cmd.extend(["--filter", args.filter])

    print(f"Targeting Log File: {TRX_FILE}")
    if args.record_failed_results:
        print(f"Recording Failures to: {FAIL_LOG_FILE}")

    if args.run_until_fail:
        print("Mode: Run until failure detected")
    else:
        print(f"Mode: Run {args.runs} times")
    print("Starting...")

    while True:
        if not args.run_until_fail and args.runs > 0 and run_count >= args.runs:
            break

        if args.run_until_fail and failed:
            break
            
        run_count += 1
        print(f"Run {run_count}...", end='\r')
        
        proc = subprocess.run(cmd, stdout=subprocess.DEVNULL, stderr=subprocess.PIPE)
        
        if os.path.exists(TRX_FILE):
            parse_trx(TRX_FILE, args.record_failed_results)
            os.remove(TRX_FILE)
        else:
            print(f"\n[!] Run {run_count} failed to produce results.")
            print("STDERR Output:")
            print(proc.stderr.decode())
            break

    print_report()
    cleanup()

if __name__ == "__main__":
    main()
