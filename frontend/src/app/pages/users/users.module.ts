import { UserProjectAssignmentComponent } from './project-assignment/project-assignment.component';
import { NgModule } from '@angular/core';

import { UsersFormComponent } from './form/users-form.component';
import { SharedModule } from '../../shared/shared.module';
import { UsersRoutingModule } from './users-routing.module';
import { UsersComponent } from './users.component';

@NgModule({
    imports: [
        UsersRoutingModule,
        SharedModule
    ],
    declarations: [
        UsersComponent,
        UsersFormComponent,
        UserProjectAssignmentComponent
    ],
    exports: [
        UsersComponent
    ]
})

export class UsersModule {
}
