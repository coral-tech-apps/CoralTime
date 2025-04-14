export class NotAssignedJiraProject{
  key: string;
  name: string;

  constructor(data = null) {
		if (data) {
			this.key = data.key;
			this.name = data.name;
		}
	}
}
