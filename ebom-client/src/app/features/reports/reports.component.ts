import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Store } from '@ngrx/store';
import { 
  DxDataGridModule, 
  DxChartModule, 
  DxSelectBoxModule, 
  DxButtonModule,
  DxDateBoxModule,
  DxTabPanelModule
} from 'devextreme-angular';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { AppState } from '../../store/app.state';
import { loadEntities } from '../../store/entities/entity.actions';

interface ReportData {
  entityType: string;
  entityName: string;
  templateRevisions: number;
  dataRevisions: number;
  lastDataUpload: string;
  totalRecords: number;
}

interface SystemMetrics {
  totalEntities: number;
  entitiesWithTemplates: number;
  entitiesWithData: number;
  totalTemplateRevisions: number;
  totalDataRevisions: number;
  totalDataRecords: number;
}

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [
    CommonModule,
    DxDataGridModule,
    DxChartModule,
    DxSelectBoxModule,
    DxButtonModule,
    DxDateBoxModule,
    DxTabPanelModule
  ],
  template: `
    <div class="p-6">
      <h1 class="text-2xl font-bold mb-6">Reports & Analytics</h1>

      <dx-tab-panel
        [deferRendering]="false"
        [loop]="false"
        [animationEnabled]="true"
        [swipeEnabled]="false">
        
        <!-- System Overview Tab -->
        <dxi-item title="System Overview">
          <div class="space-y-6">
            <!-- KPI Cards -->
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
              <div class="bg-white p-6 rounded-lg shadow">
                <div class="flex items-center">
                  <div class="p-3 rounded-full bg-blue-100">
                    <i class="dx-icon-folder text-2xl text-blue-600"></i>
                  </div>
                  <div class="ml-4">
                    <p class="text-sm font-medium text-gray-500">Total Entities</p>
                    <p class="text-2xl font-bold text-gray-900">{{ systemMetrics?.totalEntities || 0 }}</p>
                  </div>
                </div>
              </div>
              
              <div class="bg-white p-6 rounded-lg shadow">
                <div class="flex items-center">
                  <div class="p-3 rounded-full bg-green-100">
                    <i class="dx-icon-check text-2xl text-green-600"></i>
                  </div>
                  <div class="ml-4">
                    <p class="text-sm font-medium text-gray-500">With Templates</p>
                    <p class="text-2xl font-bold text-gray-900">{{ systemMetrics?.entitiesWithTemplates || 0 }}</p>
                  </div>
                </div>
              </div>
              
              <div class="bg-white p-6 rounded-lg shadow">
                <div class="flex items-center">
                  <div class="p-3 rounded-full bg-purple-100">
                    <i class="dx-icon-import text-2xl text-purple-600"></i>
                  </div>
                  <div class="ml-4">
                    <p class="text-sm font-medium text-gray-500">With Data</p>
                    <p class="text-2xl font-bold text-gray-900">{{ systemMetrics?.entitiesWithData || 0 }}</p>
                  </div>
                </div>
              </div>
              
              <div class="bg-white p-6 rounded-lg shadow">
                <div class="flex items-center">
                  <div class="p-3 rounded-full bg-orange-100">
                    <i class="dx-icon-chart text-2xl text-orange-600"></i>
                  </div>
                  <div class="ml-4">
                    <p class="text-sm font-medium text-gray-500">Total Records</p>
                    <p class="text-2xl font-bold text-gray-900">{{ formatNumber(systemMetrics?.totalDataRecords || 0) }}</p>
                  </div>
                </div>
              </div>
            </div>

            <!-- Charts -->
            <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
              <div class="bg-white p-6 rounded-lg shadow">
                <h3 class="text-lg font-semibold mb-4">Entities by Type</h3>
                <dx-chart
                  [dataSource]="entityTypeChartData"
                  type="doughnut"
                  height="300">
                  <dxi-series
                    argumentField="entityType"
                    valueField="count"
                    [label]="{ visible: true, connector: { visible: true } }">
                  </dxi-series>
                  <dxo-legend
                    orientation="horizontal"
                    [horizontalAlignment]="'center'"
                    [verticalAlignment]="'bottom'">
                  </dxo-legend>
                </dx-chart>
              </div>

              <div class="bg-white p-6 rounded-lg shadow">
                <h3 class="text-lg font-semibold mb-4">Data Upload Activity</h3>
                <dx-chart
                  [dataSource]="uploadActivityData"
                  height="300">
                  <dxi-series
                    valueField="uploads"
                    argumentField="date"
                    type="spline"
                    [color]="'#3b82f6'">
                  </dxi-series>
                  <dxo-argument-axis
                    argumentType="datetime"
                    [tickInterval]="'day'"
                    [label]="{ format: 'MMM dd' }">
                  </dxo-argument-axis>
                </dx-chart>
              </div>
            </div>
          </div>
        </dxi-item>

        <!-- Entity Reports Tab -->
        <dxi-item title="Entity Reports">
          <div class="space-y-6">
            <!-- Filters -->
            <div class="bg-white p-4 rounded-lg shadow">
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Entity Type</label>
                  <dx-select-box
                    [dataSource]="entityTypes"
                    valueExpr="value"
                    displayExpr="text"
                    [(value)]="selectedEntityType"
                    (onValueChanged)="onFilterChange()"
                    placeholder="All Types">
                  </dx-select-box>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Date From</label>
                  <dx-date-box
                    [(value)]="dateFrom"
                    (onValueChanged)="onFilterChange()"
                    [showClearButton]="true">
                  </dx-date-box>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-2">Date To</label>
                  <dx-date-box
                    [(value)]="dateTo"
                    (onValueChanged)="onFilterChange()"
                    [showClearButton]="true">
                  </dx-date-box>
                </div>
              </div>
            </div>

            <!-- Report Data Grid -->
            <div class="bg-white rounded-lg shadow">
              <dx-data-grid
                [dataSource]="reportData"
                [showBorders]="true"
                [columnAutoWidth]="true"
                [allowColumnReordering]="true"
                [allowColumnResizing]="true"
                [columnResizingMode]="'widget'"
                [showColumnLines]="true"
                [showRowLines]="true"
                [rowAlternationEnabled]="true">
                
                <dxo-paging [pageSize]="20"></dxo-paging>
                <dxo-pager
                  [showPageSizeSelector]="true"
                  [allowedPageSizes]="[10, 20, 50, 100]"
                  [showInfo]="true">
                </dxo-pager>
                
                <dxo-export [enabled]="true" [fileName]="'entity-report'"></dxo-export>
                
                <dxi-column dataField="entityType" caption="Type" [width]="80"></dxi-column>
                <dxi-column dataField="entityName" caption="Entity Name" [width]="200"></dxi-column>
                <dxi-column dataField="templateRevisions" caption="Template Revisions" [width]="120" dataType="number"></dxi-column>
                <dxi-column dataField="dataRevisions" caption="Data Revisions" [width]="120" dataType="number"></dxi-column>
                <dxi-column dataField="totalRecords" caption="Total Records" [width]="120" dataType="number" [format]="'#,##0'"></dxi-column>
                <dxi-column dataField="lastDataUpload" caption="Last Data Upload" [width]="150" dataType="datetime" [format]="'MMM dd, yyyy'"></dxi-column>
              </dx-data-grid>
            </div>
          </div>
        </dxi-item>
      </dx-tab-panel>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }
  `]
})
export class ReportsComponent implements OnInit {
  systemMetrics: SystemMetrics | null = null;
  reportData: ReportData[] = [];
  entityTypeChartData: any[] = [];
  uploadActivityData: any[] = [];

  // Filters
  entityTypes = [
    { value: null, text: 'All Types' },
    { value: 'ISR', text: 'ISR - Installation Specific Requirements' },
    { value: 'PSR', text: 'PSR - Product Specific Requirements' },
    { value: 'CSR', text: 'CSR - Customer Specific Requirements' },
    { value: 'CMN', text: 'CMN - Common Requirements' }
  ];

  selectedEntityType: string | null = null;
  dateFrom: Date | null = null;
  dateTo: Date | null = null;

  constructor(
    private store: Store<AppState>,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.store.dispatch(loadEntities());
    this.loadReportData();
    this.loadSystemMetrics();
    this.loadChartData();
  }

  async loadReportData(): Promise<void> {
    try {
      const response = await this.http.get<ReportData[]>(`${environment.apiUrl}/api/reports/entities`).toPromise();
      this.reportData = response || [];
    } catch (error) {
      console.error('Error loading report data:', error);
      // Mock data for development
      this.reportData = [
        {
          entityType: 'PSR',
          entityName: 'MotorSelection',
          templateRevisions: 3,
          dataRevisions: 5,
          lastDataUpload: '2024-01-15',
          totalRecords: 1250
        },
        {
          entityType: 'ISR',
          entityName: 'Installation_NYC',
          templateRevisions: 2,
          dataRevisions: 8,
          lastDataUpload: '2024-01-20',
          totalRecords: 890
        }
      ];
    }
  }

  async loadSystemMetrics(): Promise<void> {
    try {
      const response = await this.http.get<SystemMetrics>(`${environment.apiUrl}/api/reports/metrics`).toPromise();
      this.systemMetrics = response || null;
    } catch (error) {
      console.error('Error loading system metrics:', error);
      // Mock data for development
      this.systemMetrics = {
        totalEntities: 24,
        entitiesWithTemplates: 18,
        entitiesWithData: 15,
        totalTemplateRevisions: 42,
        totalDataRevisions: 156,
        totalDataRecords: 48750
      };
    }
  }

  loadChartData(): void {
    // Mock chart data
    this.entityTypeChartData = [
      { entityType: 'PSR', count: 8 },
      { entityType: 'ISR', count: 6 },
      { entityType: 'CSR', count: 5 },
      { entityType: 'CMN', count: 5 }
    ];

    // Generate mock upload activity data for last 30 days
    this.uploadActivityData = [];
    const today = new Date();
    for (let i = 29; i >= 0; i--) {
      const date = new Date(today);
      date.setDate(date.getDate() - i);
      this.uploadActivityData.push({
        date: date,
        uploads: Math.floor(Math.random() * 10) + 1
      });
    }
  }

  onFilterChange(): void {
    // Apply filters to report data
    console.log('Filters changed:', {
      entityType: this.selectedEntityType,
      dateFrom: this.dateFrom,
      dateTo: this.dateTo
    });
  }

  formatNumber(value: number): string {
    return value.toLocaleString();
  }
}