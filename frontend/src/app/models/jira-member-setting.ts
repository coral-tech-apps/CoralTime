export class JiraMemberSetting{
  id: number;
  userEmail: string;
	settingName: string;
	domain: string;
  apiToken: string;

	constructor(data = null) {
		if (data) {
      this.id
			this.id = data.id;
      this.userEmail = data.userEmail;
			this.settingName = data.settingName;
			this.domain = data.domain;
      this.apiToken = data.apiToken;
		}
	}
}
