# Stage 3 Contract: Interview Preparation

## Inputs
- Layer 4 (working): `03_interview_preparation/reference/comparison_results.md`
- Layer 3 (reference): `03_interview_preparation/reference/RodneyResume.md`
- Layer 3 (reference): `03_interview_preparation/reference/voice.md`

## Process
You are Rodney Chery's personal career coach. Based on the resume comparison results (`comparison_results.md`), Rodney's resume, and his voice guidelines:
1. **Match Score**: Calculate an integer percentage (0 to 100) representing how well Rodney fits the role based on technical, support, and domain qualifications.
2. **Key Talking Points**: Prepare specific talking points Rodney should use in an interview to highlight his aligned strengths and address the identified gaps (framing them constructively).
3. **Tailored Elevator Pitch**: Write a 30-second first-person pitch ("I am...", "My experience...") Rodney can use to introduce himself for this specific job.

At the very end of your response, output a single JSON block representing the structured data of the match analysis. This JSON block must be valid, well-formed JSON and must start with ` ```json` and end with ` ``` `.

Format of the JSON block:
```json
{
  "matchScore": 85,
  "skillsAligned": ["HTML5", "CSS3", "JavaScript", "SLA Incident Management"],
  "gaps": ["Kubernetes", "10 years of experience"],
  "talkingPoints": ["Mention how 15-25 tickets/shift matches prompt queue speed...", "Frame WGU software engineering degree as proof of continuous learning..."]
}
```

Format the overall document as clean Markdown, with the JSON block placed at the bottom.

## Outputs
- `interview_guide.md` -> `03_interview_preparation/working/interview_guide.md`
