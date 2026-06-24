#!/usr/bin/env python3
import os
import sys
import json
import urllib.request
import urllib.error
import argparse

def get_openai_api_key():
    return os.environ.get("OPENAI_API_KEY")

def call_openai(prompt, api_key, model="gpt-4o-mini"):
    url = "https://api.openai.com/v1/chat/completions"
    headers = {
        "Content-Type": "application/json",
        "Authorization": f"Bearer {api_key}"
    }
    data = {
        "model": model,
        "messages": [{"role": "user", "content": prompt}],
        "max_tokens": 2000
    }
    
    req = urllib.request.Request(url, data=json.dumps(data).encode("utf-8"), headers=headers)
    try:
        with urllib.request.urlopen(req) as response:
            res_data = json.loads(response.read().decode("utf-8"))
            return res_data["choices"][0]["message"]["content"].strip()
    except urllib.error.HTTPError as e:
        print(f"API Error: {e.code} - {e.read().decode('utf-8')}")
        sys.exit(1)
    except Exception as e:
        print(f"Connection Error: {e}")
        sys.exit(1)

def safe_read(file_path):
    if not os.path.exists(file_path):
        return ""
    with open(file_path, "r", encoding="utf-8") as f:
        return f.read()

def safe_write(file_path, content):
    os.makedirs(os.path.dirname(file_path), exist_ok=True)
    with open(file_path, "w", encoding="utf-8") as f:
        f.write(content)

def run_stage_1(workspace_root, job_desc, api_key):
    print("Executing Stage 1: Skill Extraction...")
    job_desc_path = os.path.join(workspace_root, "01_skill_extraction", "output", "job_description.txt")
    if job_desc:
        safe_write(job_desc_path, job_desc)
    
    job_desc_content = safe_read(job_desc_path)
    if not job_desc_content:
        print("Error: Job description missing at 01_skill_extraction/output/job_description.txt")
        sys.exit(1)
        
    global_identity = safe_read(os.path.join(workspace_root, "_config", "global_identity.md"))
    task_routing = safe_read(os.path.join(workspace_root, "_config", "task_routing.md"))
    contract = safe_read(os.path.join(workspace_root, "01_skill_extraction", "CONTEXT.md"))
    
    prompt = f"""
Identity Context:
{global_identity}
 
Routing Context:
{task_routing}
 
Stage Contract:
{contract}
 
Inputs:
- job_description.txt:
{job_desc_content}
 
Please execute the process described in the Stage Contract. Provide only the Markdown response for extracted_requirements.md."""
    
    output = call_openai(prompt, api_key)
    out_path = os.path.join(workspace_root, "01_skill_extraction", "output", "extracted_requirements.md")
    safe_write(out_path, output)
    print(f"Stage 1 complete. Saved to: {out_path}")
    return output

def run_stage_2(workspace_root, api_key):
    print("Executing Stage 2: Resume Comparison...")
    s1_output_path = os.path.join(workspace_root, "01_skill_extraction", "output", "extracted_requirements.md")
    s2_ref_path = os.path.join(workspace_root, "02_resume_comparison", "output", "extracted_requirements.md")
    
    if not os.path.exists(s1_output_path):
        print(f"Error: Stage 1 output missing at {s1_output_path}")
        sys.exit(1)
        
    # State handoff
    os.makedirs(os.path.dirname(s2_ref_path), exist_ok=True)
    import shutil
    shutil.copyfile(s1_output_path, s2_ref_path)
    
    # Copy resume context (Layer 3 reference)
    resume_src = os.path.join(workspace_root, "_config", "skills", "RodneyResume.md")
    resume_dest = os.path.join(workspace_root, "02_resume_comparison", "references", "RodneyResume.md")
    os.makedirs(os.path.dirname(resume_dest), exist_ok=True)
    shutil.copyfile(resume_src, resume_dest)
    
    global_identity = safe_read(os.path.join(workspace_root, "_config", "global_identity.md"))
    task_routing = safe_read(os.path.join(workspace_root, "_config", "task_routing.md"))
    contract = safe_read(os.path.join(workspace_root, "02_resume_comparison", "CONTEXT.md"))
    requirements_text = safe_read(s2_ref_path)
    resume_text = safe_read(resume_dest)
    
    prompt = f"""
Identity Context:
{global_identity}
 
Routing Context:
{task_routing}
 
Stage Contract:
{contract}
 
Inputs:
- RodneyResume.md:
{resume_text}
 
- extracted_requirements.md:
{requirements_text}
 
Please execute the process described in the Stage Contract. Provide only the Markdown response for comparison_results.md."""
    
    output = call_openai(prompt, api_key)
    out_path = os.path.join(workspace_root, "02_resume_comparison", "output", "comparison_results.md")
    safe_write(out_path, output)
    print(f"Stage 2 complete. Saved to: {out_path}")
    return output

def run_stage_3(workspace_root, api_key):
    print("Executing Stage 3: Interview Preparation...")
    s2_output_path = os.path.join(workspace_root, "02_resume_comparison", "output", "comparison_results.md")
    s3_ref_path = os.path.join(workspace_root, "03_interview_preparation", "output", "comparison_results.md")
    
    if not os.path.exists(s2_output_path):
        print(f"Error: Stage 2 output missing at {s2_output_path}")
        sys.exit(1)
        
    # State handoff
    os.makedirs(os.path.dirname(s3_ref_path), exist_ok=True)
    import shutil
    shutil.copyfile(s2_output_path, s3_ref_path)
    
    # Copy reference docs (Layer 3 references)
    resume_dest = os.path.join(workspace_root, "03_interview_preparation", "references", "RodneyResume.md")
    voice_dest = os.path.join(workspace_root, "03_interview_preparation", "references", "voice.md")
    os.makedirs(os.path.dirname(resume_dest), exist_ok=True)
    
    shutil.copyfile(os.path.join(workspace_root, "_config", "skills", "RodneyResume.md"), resume_dest)
    shutil.copyfile(os.path.join(workspace_root, "_config", "voice.md"), voice_dest)
    
    global_identity = safe_read(os.path.join(workspace_root, "_config", "global_identity.md"))
    task_routing = safe_read(os.path.join(workspace_root, "_config", "task_routing.md"))
    contract = safe_read(os.path.join(workspace_root, "03_interview_preparation", "CONTEXT.md"))
    comparison_text = safe_read(s3_ref_path)
    resume_text = safe_read(resume_dest)
    voice_text = safe_read(voice_dest)
    
    prompt = f"""
Identity Context:
{global_identity}
 
Routing Context:
{task_routing}
 
Stage Contract:
{contract}
 
Inputs:
- voice.md:
{voice_text}
 
- RodneyResume.md:
{resume_text}
 
- comparison_results.md:
{comparison_text}
 
Please execute the process described in the Stage Contract. Provide the Markdown response for interview_guide.md and append the JSON block containing the structured output at the very end."""
    
    output = call_openai(prompt, api_key)
    out_path = os.path.join(workspace_root, "03_interview_preparation", "output", "interview_guide.md")
    safe_write(out_path, output)
    print(f"Stage 3 complete. Saved to: {out_path}")
    return output

def main():
    parser = argparse.ArgumentParser(description="arXiv:2603.16021 ICM Pipeline Script Runner")
    parser.path = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    workspace_root = os.path.join(parser.path, "IcmWorkspace")
    
    parser.add_argument("-d", "--description", type=str, help="Raw text of the job description input for Stage 1")
    parser.add_argument("-s", "--stage", type=int, choices=[1, 2, 3], help="Specify a single target stage to run (human-in-the-loop mode)")
    
    args = parser.parse_args()
    
    api_key = get_openai_api_key()
    if not api_key:
        print("Error: OPENAI_API_KEY environment variable not configured.")
        sys.exit(1)
        
    stage = args.stage
    
    if stage == 1:
        run_stage_1(workspace_root, args.description, api_key)
    elif stage == 2:
        run_stage_2(workspace_root, api_key)
    elif stage == 3:
        run_stage_3(workspace_root, api_key)
    else:
        # Run all stages sequentially
        print("Running full sequential ICM pipeline...")
        run_stage_1(workspace_root, args.description, api_key)
        run_stage_2(workspace_root, api_key)
        run_stage_3(workspace_root, api_key)
        print("ICM pipeline run finished successfully!")

if __name__ == "__main__":
    main()
