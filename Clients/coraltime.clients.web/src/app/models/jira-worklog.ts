export class JiraWorklog{
  selected: boolean;
  date: string;
	description: string;
  projectName: string;
  projectId: number;
  timeActualString: string;
	timeActual: number;
  key: string;
  taskId: number;
  worklogId: string;
  jiraProjectId: number | null;
  type: JiraWorklogType;
  oldDate: string | undefined;
  oldTimeActual: number | undefined;
  oldTimeActualString: string | undefined;
  oldDescription: string | undefined;

	constructor(data = null) {
		if (data) {
			this.date = data.date;
			this.description = data.description;
			this.timeActualString = this.formatSecondsToTime(data.timeActual);
      this.timeActual = data.timeActual;
      this.projectName = data.projectName;
      this.key = data.key;
      this.projectId = data.projectId;
      this.worklogId = data.worklogId;
      this.type = data.type;
      this.jiraProjectId = data.jiraProjectId;
      this.oldDate = data.oldDate;
      this.oldTimeActualString = data.oldTimeActual ? this.formatSecondsToTime(data.oldTimeActual) : undefined;
      this.oldDescription = data.oldDescription;
		}
	}

  private formatSecondsToTime(totalSecond: number): string{
    const hours = Math.floor(totalSecond / 3600);
    const minutes = Math.floor((totalSecond % 3600) / 60);

    const paddedMinutes = minutes.toString().padStart(2, '0');
    return `${hours}:${paddedMinutes}`;
  }
}

export enum JiraWorklogType {
  New,
  Edited,
  Deleted
}
