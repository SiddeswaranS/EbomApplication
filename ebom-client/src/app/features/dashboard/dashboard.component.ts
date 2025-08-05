import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DxChartModule, DxPieChartModule, DxDataGridModule, DxCircularGaugeModule } from 'devextreme-angular';
import { DashboardService } from './dashboard.service';

interface KpiData {
  title: string;
  value: string;
  percentage: number;
  trend?: 'up' | 'down' | 'stable';
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, DxChartModule, DxPieChartModule, DxDataGridModule, DxCircularGaugeModule],
  template: `
    <div class="p-6">
      <h1 class="text-2xl font-bold mb-6">EBOM Dashboard</h1>
      
      <!-- KPI Cards -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <div class="bg-white rounded-lg shadow p-6" *ngFor="let kpi of kpis">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-gray-600">{{ kpi.title }}</p>
              <p class="text-2xl font-semibold">{{ kpi.value }}</p>
              <div class="flex items-center mt-2" *ngIf="kpi.trend">
                <i [class]="getTrendIcon(kpi.trend)" [style.color]="getTrendColor(kpi.trend)"></i>
                <span class="text-sm ml-1" [style.color]="getTrendColor(kpi.trend)">
                  {{ kpi.percentage }}%
                </span>
              </div>
            </div>
            <dx-circular-gauge
              [value]="kpi.percentage"
              [startValue]="0"
              [endValue]="100"
              [elementAttr]="{ class: 'gauge-small' }">
              <dxo-size [width]="80" [height]="80"></dxo-size>
              <dxi-range [startValue]="0" [endValue]="100" [color]="'#2196f3'"></dxi-range>
            </dx-circular-gauge>
          </div>
        </div>
      </div>

      <!-- Charts Section -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
        <!-- Upload Activity Chart -->
        <div class="bg-white rounded-lg shadow p-6">
          <h3 class="text-lg font-semibold mb-4">Upload Activity (Last 7 Days)</h3>
          <dx-chart
            [dataSource]="uploadActivityData"
            [elementAttr]="{ class: 'chart-container' }">
            <dxi-series 
              valueField="templates" 
              name="Templates" 
              argumentField="date"
              type="line"
              [point]="{ visible: true }">
            </dxi-series>
            <dxi-series 
              valueField="data" 
              name="Data Files" 
              argumentField="date"
              type="line"
              [point]="{ visible: true }">
            </dxi-series>
            <dxo-common-series-settings 
              [argumentField]="'date'">
            </dxo-common-series-settings>
            <dxo-legend 
              [visible]="true" 
              horizontalAlignment="center"
              verticalAlignment="bottom">
            </dxo-legend>
            <dxo-argument-axis>
              <dxo-label [customizeText]="customizeDateLabel"></dxo-label>
            </dxo-argument-axis>
            <dxo-tooltip [enabled]="true"></dxo-tooltip>
          </dx-chart>
        </div>

        <!-- Entity Distribution -->
        <div class="bg-white rounded-lg shadow p-6">
          <h3 class="text-lg font-semibold mb-4">Entity Distribution</h3>
          <dx-pie-chart
            [dataSource]="entityDistribution"
            [palette]="'Bright'"
            [elementAttr]="{ class: 'chart-container' }">
            <dxi-series
              argumentField="type"
              valueField="count">
              <dxo-label 
                [visible]="true" 
                [format]="{ type: 'percent', precision: 1 }">
                <dxo-connector [visible]="true"></dxo-connector>
              </dxo-label>
            </dxi-series>
            <dxo-legend 
              [visible]="true"
              horizontalAlignment="right"
              verticalAlignment="middle">
            </dxo-legend>
            <dxo-tooltip [enabled]="true"></dxo-tooltip>
          </dx-pie-chart>
        </div>
      </div>

      <!-- Recent Activities -->
      <div class="bg-white rounded-lg shadow">
        <div class="p-6">
          <h3 class="text-lg font-semibold mb-4">Recent Activities</h3>
          <dx-data-grid
            [dataSource]="recentActivities"
            [showBorders]="true"
            [rowAlternationEnabled]="true"
            [columnAutoWidth]="true">
            <dxo-paging [enabled]="false"></dxo-paging>
            <dxo-scrolling mode="standard"></dxo-scrolling>
            
            <dxi-column dataField="timestamp" dataType="datetime" format="MM/dd/yyyy HH:mm"></dxi-column>
            <dxi-column dataField="action" caption="Action"></dxi-column>
            <dxi-column dataField="entity" caption="Entity"></dxi-column>
            <dxi-column dataField="user" caption="User"></dxi-column>
            <dxi-column dataField="status" caption="Status" cellTemplate="statusTemplate"></dxi-column>
            
            <div *dxTemplate="let data of 'statusTemplate'">
              <span [class]="getStatusClass(data.value)">{{ data.value }}</span>
            </div>
          </dx-data-grid>
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }

    .gauge-small ::ng-deep .dx-circular-gauge {
      margin: 0;
    }

    .chart-container {
      height: 300px;
    }
  `]
})
export class DashboardComponent implements OnInit {
  kpis: KpiData[] = [
    { title: 'Total Entities', value: '156', percentage: 78, trend: 'up' },
    { title: 'Active Templates', value: '42', percentage: 92, trend: 'stable' },
    { title: 'Data Uploads Today', value: '23', percentage: 65, trend: 'up' },
    { title: 'Success Rate', value: '98.5%', percentage: 98.5, trend: 'up' }
  ];

  uploadActivityData: any[] = [];
  entityDistribution: any[] = [];
  recentActivities: any[] = [];

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.dashboardService.getDashboardData().subscribe(data => {
      this.uploadActivityData = data.uploadActivity;
      this.entityDistribution = data.entityDistribution;
      this.recentActivities = data.recentActivities;
    });
  }

  customizeDateLabel(arg: any): string {
    return new Date(arg.value).toLocaleDateString('en-US', { 
      month: 'short', 
      day: 'numeric' 
    });
  }

  getStatusClass(status: string): string {
    const classes: Record<string, string> = {
      'Success': 'text-green-600 bg-green-100 px-2 py-1 rounded text-sm',
      'Failed': 'text-red-600 bg-red-100 px-2 py-1 rounded text-sm',
      'Processing': 'text-yellow-600 bg-yellow-100 px-2 py-1 rounded text-sm'
    };
    return classes[status] || '';
  }

  getTrendIcon(trend: string): string {
    const icons: Record<string, string> = {
      'up': 'dx-icon-arrowup',
      'down': 'dx-icon-arrowdown',
      'stable': 'dx-icon-minus'
    };
    return icons[trend] || '';
  }

  getTrendColor(trend: string): string {
    const colors: Record<string, string> = {
      'up': '#10b981',
      'down': '#ef4444',
      'stable': '#6b7280'
    };
    return colors[trend] || '';
  }
}