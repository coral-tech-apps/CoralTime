export class AssignedJiraProject{
  id: number;
  jiraProjectName: string;
  projectName: string;

  constructor(data = null) {
		if (data) {
      this.id = data.id;
			this.jiraProjectName = data.jiraProjectName;
			this.projectName = data.projectName;
		}
	}
}
