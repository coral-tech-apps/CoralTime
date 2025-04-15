export class NotAssignedJiraProject{
  id: number;
  key: string;
  name: string;

  constructor(data = null) {
		if (data) {
      this.id = data.id;
			this.key = data.key;
			this.name = data.name;
		}
	}
}
