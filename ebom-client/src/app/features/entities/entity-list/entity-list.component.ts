import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DxDataGridModule, DxButtonModule } from 'devextreme-angular';
import { EntityService } from '../entity.service';
import { Observable } from 'rxjs';

export interface Entity {
  entityID: number;
  entityName: string;
  entityDisplayName: string;
  entityType: string;
  dataType: string;
  isActive: boolean;
  createdAt: Date;
}

@Component({
  selector: 'app-entity-list',
  standalone: true,
  imports: [CommonModule, DxDataGridModule, DxButtonModule],
  template: `
    <div class="p-6">
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-2xl font-bold">Entity Management</h1>
        <dx-button
          text="Add Entity"
          icon="plus"
          type="default"
          (onClick)="addEntity()">
        </dx-button>
      </div>

      <div class="bg-white rounded-lg shadow">
        <dx-data-grid
          [dataSource]="entities$ | async"
          [showBorders]="true"
          [rowAlternationEnabled]="true"
          [columnAutoWidth]="true"
          [allowColumnReordering]="true"
          [allowColumnResizing]="true"
          keyExpr="entityID"
          (onRowClick)="onRowClick($event)"
          [hoverStateEnabled]="true">
          
          <dxo-paging [pageSize]="20"></dxo-paging>
          <dxo-pager 
            [visible]="true"
            [showPageSizeSelector]="true" 
            [allowedPageSizes]="[10, 20, 50]"
            [showInfo]="true">
          </dxo-pager>
          <dxo-search-panel 
            [visible]="true"
            [width]="300"
            placeholder="Search entities...">
          </dxo-search-panel>
          <dxo-group-panel [visible]="true"></dxo-group-panel>
          <dxo-export 
            [enabled]="true" 
            [allowExportSelectedData]="true"
            [formats]="['xlsx', 'pdf']">
          </dxo-export>
          <dxo-filter-row [visible]="true"></dxo-filter-row>
          <dxo-header-filter [visible]="true"></dxo-header-filter>
          
          <dxi-column dataField="entityID" caption="ID" [width]="80" [allowFiltering]="false"></dxi-column>
          <dxi-column dataField="entityName" caption="Name" [minWidth]="150"></dxi-column>
          <dxi-column dataField="entityDisplayName" caption="Display Name" [minWidth]="150"></dxi-column>
          <dxi-column dataField="entityType" caption="Type" [width]="100">
            <dxo-lookup 
              [dataSource]="entityTypes"
              valueExpr="value"
              displayExpr="text">
            </dxo-lookup>
          </dxi-column>
          <dxi-column dataField="dataType" caption="Data Type" [width]="120"></dxi-column>
          <dxi-column 
            dataField="isActive" 
            caption="Active" 
            dataType="boolean" 
            [width]="80"
            cellTemplate="activeTemplate">
          </dxi-column>
          <dxi-column 
            dataField="createdAt" 
            caption="Created" 
            dataType="datetime"
            format="MM/dd/yyyy"
            [width]="120">
          </dxi-column>
          
          <dxi-column 
            type="buttons" 
            [width]="120"
            caption="Actions">
            <dxi-button 
              name="edit" 
              icon="edit" 
              hint="Edit"
              [onClick]="editEntity">
            </dxi-button>
            <dxi-button 
              name="delete" 
              icon="trash" 
              hint="Delete"
              [onClick]="deleteEntity">
            </dxi-button>
          </dxi-column>

          <div *dxTemplate="let data of 'activeTemplate'">
            <span 
              class="px-2 py-1 text-xs rounded-full"
              [ngClass]="data.value ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'">
              {{ data.value ? 'Active' : 'Inactive' }}
            </span>
          </div>
        </dx-data-grid>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }
    
    ::ng-deep .dx-datagrid-rowsview .dx-row {
      cursor: pointer;
    }
    
    ::ng-deep .dx-datagrid-rowsview .dx-row:hover {
      background-color: #f3f4f6;
    }
  `]
})
export class EntityListComponent implements OnInit {
  entities$: Observable<Entity[]>;
  
  entityTypes = [
    { value: 'ISR', text: 'Input Selection Rule' },
    { value: 'PSR', text: 'Part Selection Rule' },
    { value: 'CSR', text: 'Cost Selection Rule' },
    { value: 'CMN', text: 'Common' }
  ];

  constructor(
    private entityService: EntityService,
    private router: Router
  ) {
    this.entities$ = this.entityService.getEntities();
  }

  ngOnInit(): void {
    this.loadEntities();
  }

  loadEntities(): void {
    this.entities$ = this.entityService.getEntities();
  }

  onRowClick(e: any): void {
    if (e.rowType === 'data' && e.event && !e.event.target.closest('.dx-command-edit')) {
      this.router.navigate(['/entities', e.data.entityID]);
    }
  }

  addEntity(): void {
    this.router.navigate(['/entities/new']);
  }

  editEntity = (e: any) => {
    e.event.stopPropagation();
    this.router.navigate(['/entities', e.row.data.entityID, 'edit']);
  }

  deleteEntity = (e: any) => {
    e.event.stopPropagation();
    if (confirm(`Are you sure you want to delete ${e.row.data.entityDisplayName}?`)) {
      this.entityService.deleteEntity(e.row.data.entityID).subscribe({
        next: () => {
          this.loadEntities();
        },
        error: (error) => {
          console.error('Error deleting entity:', error);
        }
      });
    }
  }
}