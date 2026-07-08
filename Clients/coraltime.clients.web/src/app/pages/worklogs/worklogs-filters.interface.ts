import { JiraMemberSetting } from "src/app/models/jira-member-setting";
import { DateResponse } from "../reports/range-datepicker/range-datepicker.service";

export interface WorklogFilters {
  dateRange: DateResponse;
  jiraMemberSetting: JiraMemberSetting;
  selectedJiraProjectIds: number[];
}