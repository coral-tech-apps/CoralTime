export class AssignedJiraProject{
  jiraProjectName: string;
  projectName: string;

  constructor(data = null) {
		if (data) {
			this.jiraProjectName = data.jiraProjectName;
			this.projectName = data.projectName;
		}
	}
}
