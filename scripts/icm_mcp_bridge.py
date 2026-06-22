#!/usr/bin/env python3
import os
import sys
import argparse
import re

def parse_contract_inputs(contract_text):
    inputs = []
    section = re.search(r"## Inputs(.*?)(##|\Z)", contract_text, re.DOTALL | re.IGNORECASE)
    if section:
        lines = section.group(1).strip().split('\n')
        for line in lines:
            m = re.search(r"[`'](.*?)[`']", line)
            if m:
                inputs.append(m.group(1))
    return inputs

def get_stage_inputs(workspace_root, stage):
    stage_dirs = {
        1: "01_skill_extraction",
        2: "02_resume_comparison",
        3: "03_interview_preparation"
    }
    dir_name = stage_dirs.get(stage)
    if not dir_name:
        print(f"Error: Invalid stage {stage}")
        sys.exit(1)
        
    contract_path = os.path.join(workspace_root, dir_name, "CONTEXT.md")
    if not os.path.exists(contract_path):
        print(f"Error: Contract missing at {contract_path}")
        sys.exit(1)
        
    with open(contract_path, "r", encoding="utf-8") as f:
        contract_text = f.read()
        
    inputs = parse_contract_inputs(contract_text)
    return dir_name, inputs

def verify_stage_dependencies(workspace_root, stage):
    dir_name, inputs = get_stage_inputs(workspace_root, stage)
    print(f"Verifying dependencies for Stage {stage} ({dir_name})...")
    
    all_ok = True
    for input_file in inputs:
        # Check if it's absolute or relative to the stage folder
        if input_file.startswith("../"):
            # Resolve relative to the stage folder
            resolved_path = os.path.normpath(os.path.join(workspace_root, dir_name, input_file))
        elif "/" in input_file:
            # Path relative to workspace root or direct relative
            resolved_path = os.path.normpath(os.path.join(workspace_root, dir_name, "reference", os.path.basename(input_file)))
        else:
            resolved_path = os.path.normpath(os.path.join(workspace_root, dir_name, "reference", input_file))
            
        exists = os.path.exists(resolved_path)
        status = "OK" if exists else "MISSING"
        print(f"  - Dependency: {input_file} -> {resolved_path} [{status}]")
        if not exists:
            all_ok = False
            
    if all_ok:
        print("Status: Success - All stage dependencies are resolved.")
    else:
        print("Status: Failed - One or more dependencies are missing from the reference workspace.")
    return all_ok

def main():
    parser = argparse.ArgumentParser(description="ICM Agent Capabilities Bridge / Dependency Resolver")
    parser.path = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    workspace_root = os.path.join(parser.path, "IcmWorkspace")
    
    subparsers = parser.add_subparsers(dest="command", required=True)
    
    # Inputs list command
    inputs_parser = subparsers.add_parser("get-inputs", help="Retrieve stage inputs from contract")
    inputs_parser.add_argument("-s", "--stage", type=int, choices=[1, 2, 3], required=True, help="Stage sequence number")
    
    # Verify dependencies command
    verify_parser = subparsers.add_parser("verify-deps", help="Verify all input dependency files exist for a stage")
    verify_parser.add_argument("-s", "--stage", type=int, choices=[1, 2, 3], required=True, help="Stage sequence number")
    
    args = parser.parse_args()
    
    if args.command == "get-inputs":
        dir_name, inputs = get_stage_inputs(workspace_root, args.stage)
        print(f"Stage {args.stage} ({dir_name}) Contract Inputs:")
        for inp in inputs:
            print(f"  - {inp}")
    elif args.command == "verify-deps":
        success = verify_stage_dependencies(workspace_root, args.stage)
        sys.exit(0 if success else 1)

if __name__ == "__main__":
    main()
