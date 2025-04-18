import { SharedModule } from '../../shared/shared.module';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { JiraIntegrationComponent } from './jira-integration.component';
import { JiraIntegrationRoutingModule } from './jira-integration-routin.module';
import { JiraUsersComponent } from './jira-member-form/jira-member.component';
import { JiraLinkedProjectComponent } from './jira-linked-project/jira-linked-project.component';

@NgModule({
    imports: [
      CommonModule,
      SharedModule,
      JiraIntegrationRoutingModule
    ],
    declarations: [
      JiraIntegrationComponent,
      JiraUsersComponent,
      JiraLinkedProjectComponent
    ],
    exports: [
      JiraIntegrationComponent,
      JiraUsersComponent,
      JiraLinkedProjectComponent
    ]
})

export class JiraIntegrationModule{
}
