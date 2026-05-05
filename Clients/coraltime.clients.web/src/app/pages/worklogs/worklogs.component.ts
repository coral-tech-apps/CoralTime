import { NotificationService } from './../../core/notification.service';
import { JiraProjectService } from 'src/app/services/jira-project.service';
import { finalize } from 'rxjs/operators';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import dayjs from 'dayjs';
import {
	ReportDropdowns,
  ReportQuery,
} from '../../models/reports';
import { User } from '../../models/user';
import { LoadingMaskService } from '../../shared/loading-indicator/loading-mask.service';
import { ImpersonationService } from '../../services/impersonation.service';
import { ReportsService } from '../../services/reposts.service';
import { CustomSelectItem } from '../../shared/form/multiselect/multiselect.component';
import { DatePeriod, DateResponse, RangeDatepickerService } from '../reports/range-datepicker/range-datepicker.service';
import { JiraSettingService } from 'src/app/services/jira-settings.service';
import { JiraMemberSetting } from 'src/app/models/jira-member-setting';
import { AssignedJiraProject } from 'src/app/models/assigned-jira-project';
import { ROWS_ON_PAGE } from 'src/app/core/constant.service';
import { PagedResult } from 'src/app/services/odata';
import { HttpClient } from '@angular/common/http';
import { JiraWorklog, JiraWorklogType } from 'src/app/models/jira-worklog';
import { Task } from 'src/app/models/task';
import { TasksService } from 'src/app/services/tasks.service';
import { WorkglogService } from 'src/app/services/worklog.service';

const ROWS_TOTAL_NUMBER = 50;

@Component({
    selector: 'ct-worklogs',
    templateUrl: 'worklogs.component.html',
    standalone: false
})

export class WorklogsComponent implements OnInit {
  isAvaliableCheckProject: boolean = false;
  hasSelectedWorklogsWithoutTask: boolean = false;
  hasNoJiraFiltersSelected: boolean = true;
  hasJiraFiltersButNoWorklogsSelected: boolean = false;
  isEmptyProjects: boolean = true;
	reportDropdowns: ReportDropdowns;
  user: User;

  tasks: Task[] =[]
  isTasksLoading: boolean;

  isAllSelected: boolean = false;
  worklogs: JiraWorklog[] = [];
  worklogTypes = JiraWorklogType;
  isWorklogsLoaded: boolean = true;

  jiraSettings: JiraMemberSetting[] = []
  selectedJiraSetting: JiraMemberSetting;

  assignedJiraProjectsIds: number[] = []
  assignedJiraProjectsItems: CustomSelectItem[] = []

	isDatepickerShown: boolean = false;
	isDatepickerAnimating: boolean = false;
	canToggleDatepicker: boolean = true;
	dateFormat: string;
	dateFormatId: number;
	dateResponse: DateResponse;
	dateString: string = 'This Week';
	firstDayOfWeek: number;
	oldDateResponse: DateResponse;
	oldDateString: string;
	userInfo: User;

	@ViewChild('scrollContainer') private scrollContainer: ElementRef;
	@ViewChild('slimScroll') slimScroll: any;

  private assignedProjectsEvent: any;

	constructor(
              private jiraSettingService: JiraSettingService,
              private JiraProjectService: JiraProjectService,
	            private impersonationService: ImpersonationService,
	            private loadingService: LoadingMaskService,
	            private rangeDatepickerService: RangeDatepickerService,
	            private reportsService: ReportsService,
	            private route: ActivatedRoute,
              private tasksService: TasksService,
              private notificationService: NotificationService,
              private worklogService: WorkglogService,
              private http: HttpClient) {
              this.route.data.forEach((data: { user: User }) => {
                this.user = this.impersonationService.impersonationUser || data.user;
                });
	}

	ngOnInit() {

		this.route.data.forEach((data: { user: User, reportFilters: ReportDropdowns }) => {
			this.userInfo = this.impersonationService.impersonationUser || data.user;
			this.dateFormat = this.userInfo.dateFormat;
			this.dateFormatId = this.userInfo.dateFormatId;
			this.firstDayOfWeek = this.userInfo.weekStart;
		});

    this.getJiraSettings();
    this.checkSelectProject();

		this.loadingService.addLoading();
		this.reportsService.getReportDropdowns().pipe(
			finalize(() => this.loadingService.removeLoading()))
			.subscribe((reportFilters: ReportDropdowns) => {
				this.setWorklogDropdowns(reportFilters);
			});
    this.loadTasks();

	}

	setWorklogDropdowns(worklogDropdowns: ReportDropdowns): void {
		this.reportDropdowns = worklogDropdowns;
		this.rangeDatepickerService.dateStaticList = worklogDropdowns.values.dateStatic;

    this.setWorkglogDatePeriog(worklogDropdowns.currentQuery);
	}

    private setWorkglogDatePeriog(worklogFilters: ReportQuery): void {
      this.datePeriodOnChange({
        datePeriod: new DatePeriod(dayjs(worklogFilters.dateFrom), dayjs(worklogFilters.dateTo)),
        dateStaticId: worklogFilters.dateStaticId
      });
    }

  // send timeEntries
  addTimeEntries(): void{
    const selectedWorklog = this.worklogs.filter(worklog => worklog.selected);

    if(selectedWorklog.length == 0){
      this.notificationService.danger("No worklogs selected.");
      return;
    }

    for(const worklog of selectedWorklog){
      if(worklog.taskId === undefined || worklog.taskId === 0){
        this.notificationService.danger(`No task selected for ${worklog.projectName}`);
        return;
      }
    }

    this.worklogService.sendWorklogs(selectedWorklog).subscribe(
    () => {
      this.notificationService.success("Time entries successfully loaded");
      this.applyFilters();
    },
    () => {
      this.notificationService.danger("Error while loading time entries");
    }
  )
  }

  // tasks
  private loadTasks(): void {
      this.tasksService.getActiveTasks().subscribe(result => {
        this.tasks = result.data;
      })
    }

  addTaskToWorklog(task: Task, worklogIndex: number): void{
    this.worklogs[worklogIndex].taskId = task.id;
    this.updatePopupFlags();
  }

  // checkbox

    toggleSelectAll(){
      this.worklogs.forEach(item =>
        {
          if(item.timeActual <= 86400){
            item.selected = this.isAllSelected;
          }
        });
      this.updatePopupFlags();
    }

    checkAllSelected(){
      this.isAllSelected = this.worklogs
        .filter(item => item.timeActual <= 86400)
        .every(item => item.selected);
       this.updatePopupFlags();
    }

  // Apply filters

  applyFilters(): void{
    this.isWorklogsLoaded = false;

    const fromDate = this.dateResponse.datePeriod.dateFrom.format('YYYY-MM-DD');
    const toDate = this.dateResponse.datePeriod.dateTo.format('YYYY-MM-DD');

    const projectIds = this.assignedJiraProjectsIds?.length
      ? this.assignedJiraProjectsIds
      : this.assignedJiraProjectsItems.map(item => item.value);

    const filters = {
      dateFrom: fromDate,
      dateTo: toDate,
      jiraSettingId: this.selectedJiraSetting.jiraSettingId,
      projectIds: projectIds
    };

    this.getWorklogs(filters);

  }

  getWorklogs(filters: any): void{
    this.worklogService.getWorklogs(filters)
      .subscribe((res: JiraWorklog[]) => {
        this.worklogs = res.map(item => new JiraWorklog(item));
        this.isWorklogsLoaded = true;
        this.updatePopupFlags();
      });
  }

  // Jira Setting Select

  getJiraSettings(){
    this.jiraSettingService.getJiraMemberSettings().subscribe(result => {
      this.jiraSettings = result;
    })
  }

  onClickJiraSetting(jiraSetting: JiraMemberSetting){
    this.selectedJiraSetting = jiraSetting;
    this.assignedJiraProjectsIds = [];
    this.checkSelectProject();
    this.updatePopupFlags();
  }

  // Assigned Jira Projects

  getAssignedJiraProjects(){
    if(!this.assignedProjectsEvent){
      this.assignedProjectsEvent = {
        first: 0,
        rows: ROWS_ON_PAGE
      }
    }
    this.JiraProjectService.getAssignedProjects(this.selectedJiraSetting.jiraSettingId, this.assignedProjectsEvent, '').subscribe((result: PagedResult<AssignedJiraProject>) => {
      this.assignedJiraProjectsItems = result.data.map(project => {
        return {
          value: project.id,
          label: project.jiraProjectName
        }
      })

      this.setIsEmptyProject();
    });
  }

  setIsEmptyProject(): void{
    if(this.assignedJiraProjectsItems.length > 0){
      this.isEmptyProjects = false;
    }
  }

  checkSelectProject(){
    if(this.selectedJiraSetting){
      this.isAvaliableCheckProject = true;
      this.getAssignedJiraProjects();
    }else{
      this.isAvaliableCheckProject = false;
    }
  }

	// Datepicker

	cancelUpdatingReportGrid(): void {
		this.dateString = this.oldDateString;
		this.dateResponse = this.oldDateResponse;
		this.closeRangeDatepicker();
	}

	closeRangeDatepicker(): void {
		this.isDatepickerShown = false;
		this.isDatepickerAnimating = false;
	}

	openRangeDatepicker(): void {
		this.oldDateString = this.dateString;
		this.oldDateResponse = this.dateResponse;
		this.isDatepickerShown = true;
		setTimeout(() => this.isDatepickerAnimating = true, 300);
	}

	toggleRangeDatepicker(event?: MouseEvent): void {
		if (event && (<HTMLElement>event.target).classList.contains('fa-times') || !this.canToggleDatepicker) {
			return;
		}

		if (this.isDatepickerShown) {
			this.closeRangeDatepicker();
		} else {
			this.openRangeDatepicker();
		}

		this.changeToggleParameter();
	}

	datePeriodOnChange(dateResponse: DateResponse): void {
		this.dateResponse = dateResponse;
		this.setDateString(dateResponse.datePeriod);
	}

	getNewPeriod(isNext: boolean = true): void {
		const dateFrom = this.dateResponse.datePeriod.dateFrom;
		const dateTo = this.dateResponse.datePeriod.dateTo;

		if (this.rangeDatepickerService.isIntegerNumberOfMonths(this.dateResponse.datePeriod)) {
			let monthInPeriod = isNext ? dateTo.diff(dateFrom, 'month') + 1 : -(dateTo.diff(dateFrom, 'month') + 1);
			this.dateResponse.datePeriod = new DatePeriod(
				dayjs().year(dateFrom.year()).month(dateFrom.month() + monthInPeriod).date(1),
				dayjs().year(dateTo.year()).month(dateTo.month() + monthInPeriod + 1).date(0)
			);
		} else {
			let daysInPeriod = isNext ? dateTo.diff(dateFrom, 'days') + 1 : -(dateTo.diff(dateFrom, 'days') + 1);
			this.dateResponse.datePeriod = new DatePeriod(
				dayjs().year(dateFrom.year()).month(dateFrom.month()).date(dateFrom.date() + daysInPeriod),
				dayjs().year(dateTo.year()).month(dateTo.month()).date(dateTo.date() + daysInPeriod)
			);
		}

		this.setDateString(this.dateResponse.datePeriod);
		this.dateResponse.dateStaticId = null;
	}

  isDisable(): boolean {
    return this.hasSelectedWorklogsWithoutTask || this.hasNoJiraFiltersSelected || this.hasJiraFiltersButNoWorklogsSelected;
  }

	private changeToggleParameter(): void {
		this.canToggleDatepicker = false;
		setTimeout(() => this.canToggleDatepicker = true, 300);
	}

	private setDateString(period: DatePeriod): void {
		let selectedRange = new DatePeriod(period.dateFrom, period.dateTo);
		this.dateString = this.rangeDatepickerService.setDateStringPeriod(selectedRange);
	}

  private updatePopupFlags(): void {
  this.hasSelectedWorklogsWithoutTask = this.worklogs.some(w => w.selected && (!w.taskId || w.taskId === 0));

  this.hasNoJiraFiltersSelected = !this.selectedJiraSetting ||
    (this.assignedJiraProjectsIds.length === 0 && this.assignedJiraProjectsItems.length === 0);

  this.hasJiraFiltersButNoWorklogsSelected =
    !!this.selectedJiraSetting &&
    (this.assignedJiraProjectsIds.length > 0 || this.assignedJiraProjectsItems.length > 0) &&
    this.worklogs.length > 0 &&
    this.worklogs.every(w => !w.selected);
  }
}
