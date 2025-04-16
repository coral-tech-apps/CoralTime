export class JiraWorklog{
  selected: boolean;
  date: string;
	description: string;
	timeActual: string;


	constructor(data = null) {
		if (data) {
			this.date = data.date;
			this.description = data.description;
			this.timeActual = this.formatSecondsToTime(data.timeActual);
		}
	}

  private formatSecondsToTime(totalSecond: number): string{
    const hours = Math.floor(totalSecond / 3600);
    const minutes = Math.floor((totalSecond % 3600) / 60);

    return `${hours}:${minutes}`;
  }
}
