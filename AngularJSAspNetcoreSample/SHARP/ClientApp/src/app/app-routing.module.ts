import { Routes } from '@angular/router';
import { FullComponent } from './layouts/full/full.component';
import { LoginComponent } from './login/login.component';
import { AuthGuard } from './auth.guard';
import { CodeValidationComponent } from './shared/code-validation.component';


export const Approutes: Routes = [
  {
    path: '',
    component: FullComponent,
    canActivate: [AuthGuard],
    children: [
      { path: '', redirectTo: '/home', pathMatch: 'full' },
      //hiding dashboard
      /*{
        path: 'home',
        loadChildren: () => import('./audits/audits.module').then(m => m.AuditsModule),
      },
      {
        path: 'dashboard',
        loadChildren: () => import('./dashboard/dashboard.module').then(m => m.DashboardModule),
      },*/
      {
        path: 'home',
        loadChildren: () => import('./dashboard/dashboard.module').then(m => m.DashboardModule),
      },
      {
        path: 'audits',
        loadChildren: () => import('./audits/audits.module').then(m => m.AuditsModule),
      },
      {
        path: 'report/requests',
        loadChildren: () => import('./report-request/report-request.module').then(m => m.ReportRequestModule),
      },
      {
        path: 'forms-management',
        loadChildren: () => import('./forms-management/forms-management.module').then(m => m.FormsManagementModule),
      },
      {
        path: 'component',
        loadChildren: () => import('./component/component.module').then(m => m.ComponentsModule)
      },
      {
        path: 'users',
        loadChildren: () => import('./users/users.module').then(m => m.UsersModule)
      },
      {
        path: 'organizations',
        loadChildren: () => import('./organizations/organizations.module').then(m => m.OrganizationsModule)
      },
      {
        path: 'reports',
        loadChildren: () => import('./reports/reports.module').then(m => m.ReportsModule),
      },
      {
        path: 'dashboard-input',
        loadChildren: () => import('./dashboard-input/dashboard-input.module').then(m => m.DashboardInputModule),
      },
      {
        path: 'aitool',
        loadChildren: () => import('./ai-tool/ai-tool.module').then(m => m.AiToolModule),
      },
      {
        path: 'auditor-productivity-dashboard',
        loadChildren: () => import('./auditor-productivity-dashboard/auditor-productivity-dashboard.module').then(m => m.AuditorProductivityDashboardModule),
      },
    ]
  },
  {
    path: 'login',
    component: LoginComponent,
  },
  {
    path: '.well-known/pki-validation/godaddy.html',
    component: CodeValidationComponent,
  },
  {
    path: '**',
    redirectTo: ''
  }
];

