export class JiraSetting{
  id: string;
	settingName: string;
	userEmail: string;
	domain: string;
  apiToken: string;

	constructor(data = null) {
		if (data) {
			this.id = data.id;
			this.settingName = data.settingName;
			this.userEmail = data.userEmail;
			this.domain = data.domain;
      this.apiToken = data.apiToken;
		}
	}
}
