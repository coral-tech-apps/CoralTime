export class JiraMemberSetting{
  id: number;
  userEmail: string;
	settingName: string;
  jiraSettingId: number;
  isEnableConntection: boolean;
	domain: string;
  apiToken: string;

	constructor(data = null) {
		if (data) {
			this.id = data.id;
      this.userEmail = data.userEmail;
			this.settingName = data.settingName;
      this.jiraSettingId = data.jiraSettingId;
      this.isEnableConntection = data.isEnableConntection;
      this.domain = data.domain;
      this.apiToken = data.apiToken;
		}
	}
}
