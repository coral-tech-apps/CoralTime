export class JiraSetting{
  id: number;
	settingName: string;
	domain: string;
  memberCount: number;
  clientId: number;
  clientName: string;

	constructor(data = null) {
		if (data) {
			this.id = data.id;
			this.settingName = data.settingName;
			this.domain = data.domain;
      this.memberCount = data.memberCount;
      this.clientId = data.clientId;
      this.clientName = data.clientName;
		}
	}
}
