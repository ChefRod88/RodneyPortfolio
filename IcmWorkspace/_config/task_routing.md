# Layer 1: Task Routing Configuration

This file routes user requests to specific stages of the pipeline.

## Workflows
- **Job Matching and Interview Prep**:
  1. **Stage 01 (01_skill_extraction)**: Extract core required skills and criteria from the provided Job Description.
  2. **Stage 02 (02_resume_comparison)**: Compare extracted criteria against Rodney Chery's Resume to identify alignment and gaps.
  3. **Stage 03 (03_interview_preparation)**: Synthesize comparison into a final match score, specific talking points, and a comprehensive interview preparation guide.
