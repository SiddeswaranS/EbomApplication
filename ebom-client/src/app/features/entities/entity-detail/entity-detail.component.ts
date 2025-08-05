import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DxTabPanelModule, DxFormModule, DxDataGridModule, DxButtonModule, DxTextBoxModule, DxTextAreaModule, DxSelectBoxModule, DxSwitchModule, DxTagBoxModule } from 'devextreme-angular';
import { EntityService } from '../entity.service';
import { Observable, switchMap, of } from 'rxjs';
import { Entity } from '../entity-list/entity-list.component';

interface TemplateRevision {
  templateRevisionNumber: number;
  templateRevisionDescription: string;
  isActive: boolean;
  createdAt: Date;
  createdBy: string;
}

interface EntityValue {
  entityValueId: number;
  entityObjValue: string;
  isActive: boolean;
  createdAt: Date;
}

@Component({
  selector: 'app-entity-detail',
  standalone: true,
  imports: [
    CommonModule, 
    RouterLink,
    DxTabPanelModule, 
    DxFormModule, 
    DxDataGridModule, 
    DxButtonModule,
    DxTextBoxModule,
    DxTextAreaModule,
    DxSelectBoxModule,
    DxSwitchModule,
    DxTagBoxModule
  ],
  template: `
    <div class="p-6" *ngIf="entity$ | async as entity">
      <div class="mb-6">
        <button class="text-blue-600 hover:text-blue-800 mb-4 inline-flex items-center" routerLink="/entities">
          <i class="dx-icon-chevronleft mr-1"></i> Back to Entities
        </button>
        <div class="flex justify-between items-center">
          <h1 class="text-2xl font-bold">{{ entity.entityDisplayName }}</h1>
          <div class="flex gap-2">
            <dx-button
              *ngIf="!isEditMode"
              text="Edit"
              type="default"
              icon="edit"
              (onClick)="toggleEditMode()">
            </dx-button>
            <dx-button
              *ngIf="isEditMode"
              text="Save"
              type="success"
              icon="save"
              (onClick)="saveEntity()">
            </dx-button>
            <dx-button
              *ngIf="isEditMode"
              text="Cancel"
              type="normal"
              icon="close"
              (onClick)="cancelEdit()">
            </dx-button>
          </div>
        </div>
      </div>

      <dx-tab-panel
        [height]="600"
        [animationEnabled]="true"
        [swipeEnabled]="true">
        
        <!-- General Information Tab -->
        <dxi-item title="General Information" icon="info">
          <div class="p-6">
            <dx-form 
              [formData]="editableEntity || entity" 
              [readOnly]="!isEditMode"
              [showColonAfterLabel]="true"
              labelLocation="left"
              [labelMode]="'floating'">
              
              <dxi-item dataField="entityName" 
                [label]="{ text: 'Entity Name' }"
                [editorOptions]="{ readOnly: true }">
                <dxi-validation-rule type="required"></dxi-validation-rule>
              </dxi-item>
              
              <dxi-item dataField="entityDisplayName" 
                [label]="{ text: 'Display Name' }"
                editorType="dxTextBox">
                <dxi-validation-rule type="required"></dxi-validation-rule>
              </dxi-item>
              
              <dxi-item dataField="entityDescription" 
                [label]="{ text: 'Description' }"
                editorType="dxTextArea" 
                [editorOptions]="{ height: 100 }">
              </dxi-item>
              
              <dxi-item dataField="entityType" 
                [label]="{ text: 'Entity Type' }"
                editorType="dxSelectBox"
                [editorOptions]="{ 
                  items: entityTypes, 
                  displayExpr: 'text', 
                  valueExpr: 'value',
                  readOnly: true 
                }">
              </dxi-item>
              
              <dxi-item dataField="dataType" 
                [label]="{ text: 'Data Type' }"
                [editorOptions]="{ readOnly: true }">
              </dxi-item>
              
              <dxi-item dataField="isActive" 
                [label]="{ text: 'Active' }"
                editorType="dxSwitch">
              </dxi-item>
              
              <dxi-item itemType="group" caption="Audit Information" [colCount]="2">
                <dxi-item dataField="createdAt" 
                  [label]="{ text: 'Created' }"
                  [editorOptions]="{ readOnly: true, displayFormat: 'MM/dd/yyyy HH:mm' }">
                </dxi-item>
                <dxi-item dataField="createdBy" 
                  [label]="{ text: 'Created By' }"
                  [editorOptions]="{ readOnly: true, value: 'System' }">
                </dxi-item>
              </dxi-item>
            </dx-form>
          </div>
        </dxi-item>

        <!-- Template Revisions Tab -->
        <dxi-item title="Template Revisions" icon="version">
          <div class="p-6">
            <dx-data-grid
              [dataSource]="templateRevisions$ | async"
              [showBorders]="true"
              [columnAutoWidth]="true"
              [hoverStateEnabled]="true">
              <dxo-paging [enabled]="false"></dxo-paging>
              <dxo-sorting mode="single"></dxo-sorting>
              
              <dxi-column dataField="templateRevisionNumber" caption="Revision" [width]="100"></dxi-column>
              <dxi-column dataField="templateRevisionDescription" caption="Description"></dxi-column>
              <dxi-column dataField="isActive" caption="Active" dataType="boolean" [width]="80"></dxi-column>
              <dxi-column dataField="createdAt" caption="Created" dataType="datetime" format="MM/dd/yyyy HH:mm"></dxi-column>
              <dxi-column dataField="createdBy" caption="Created By"></dxi-column>
            </dx-data-grid>
          </div>
        </dxi-item>

        <!-- Entity Values Tab -->
        <dxi-item title="Entity Values" icon="rowfield">
          <div class="p-6">
            <div class="mb-4 flex justify-between items-center">
              <p class="text-sm text-gray-600">
                Unique values stored for this entity
              </p>
              <dx-button
                text="Refresh"
                icon="refresh"
                type="normal"
                (onClick)="refreshEntityValues()">
              </dx-button>
            </div>
            <dx-data-grid
              [dataSource]="entityValues$ | async"
              [showBorders]="true"
              [columnAutoWidth]="true"
              [hoverStateEnabled]="true">
              <dxo-paging [pageSize]="20"></dxo-paging>
              <dxo-pager 
                [visible]="true"
                [showPageSizeSelector]="true" 
                [allowedPageSizes]="[10, 20, 50]">
              </dxo-pager>
              <dxo-search-panel [visible]="true"></dxo-search-panel>
              <dxo-filter-row [visible]="true"></dxo-filter-row>
              
              <dxi-column dataField="entityObjValue" caption="Value"></dxi-column>
              <dxi-column dataField="isActive" caption="Active" dataType="boolean" [width]="80"></dxi-column>
              <dxi-column dataField="createdAt" caption="Created" dataType="datetime" format="MM/dd/yyyy HH:mm" [width]="180"></dxi-column>
            </dx-data-grid>
          </div>
        </dxi-item>

        <!-- Mirror Entities Tab -->
        <dxi-item title="Mirror Entities" icon="link">
          <div class="p-6">
            <div class="mb-4">
              <p class="text-sm text-gray-600 mb-4">
                Select entities that should mirror this entity's values
              </p>
              <dx-tag-box
                [(value)]="mirrorEntityIds"
                [dataSource]="allEntities$ | async"
                displayExpr="entityDisplayName"
                valueExpr="entityID"
                [searchEnabled]="true"
                [showSelectionControls]="true"
                [disabled]="!isEditMode"
                placeholder="Select mirror entities..."
                [height]="40">
              </dx-tag-box>
            </div>
          </div>
        </dxi-item>

        <!-- Data Revisions Tab -->
        <dxi-item title="Data Revisions" icon="clock">
          <div class="p-6">
            <dx-data-grid
              [dataSource]="dataRevisions$ | async"
              [showBorders]="true"
              [columnAutoWidth]="true"
              [hoverStateEnabled]="true">
              <dxo-paging [pageSize]="10"></dxo-paging>
              <dxo-sorting mode="single"></dxo-sorting>
              
              <dxi-column dataField="dataRevisionNumber" caption="Revision" [width]="100"></dxi-column>
              <dxi-column dataField="dataRevisionDescription" caption="Description"></dxi-column>
              <dxi-column dataField="rowCount" caption="Rows" [width]="100"></dxi-column>
              <dxi-column dataField="isActive" caption="Active" dataType="boolean" [width]="80"></dxi-column>
              <dxi-column dataField="createdAt" caption="Upload Date" dataType="datetime" format="MM/dd/yyyy HH:mm"></dxi-column>
              <dxi-column dataField="createdBy" caption="Uploaded By"></dxi-column>
            </dx-data-grid>
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
export class EntityDetailComponent implements OnInit {
  entity$: Observable<Entity>;
  templateRevisions$: Observable<TemplateRevision[]> = new Observable();
  entityValues$: Observable<EntityValue[]> = new Observable();
  dataRevisions$: Observable<any[]> = new Observable();
  allEntities$: Observable<Entity[]>;
  
  isEditMode = false;
  editableEntity: Entity | null = null;
  mirrorEntityIds: number[] = [];
  
  entityTypes = [
    { value: 'ISR', text: 'Input Selection Rule' },
    { value: 'PSR', text: 'Part Selection Rule' },
    { value: 'CSR', text: 'Cost Selection Rule' },
    { value: 'CMN', text: 'Common' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private entityService: EntityService
  ) {
    this.entity$ = this.route.params.pipe(
      switchMap(params => {
        const id = +params['id'];
        return this.loadEntityDetails(id);
      })
    );

    this.allEntities$ = this.entityService.getEntities();
  }

  ngOnInit(): void {
    // Load related data
    this.templateRevisions$ = of(this.getMockTemplateRevisions());
    this.entityValues$ = of(this.getMockEntityValues());
    this.dataRevisions$ = of(this.getMockDataRevisions());
  }

  loadEntityDetails(id: number): Observable<Entity> {
    // For now, return mock data. Replace with actual API call
    return of({
      entityID: id,
      entityName: 'MotorSelection',
      entityDisplayName: 'Motor Selection',
      entityType: 'PSR',
      dataType: 'String',
      isActive: true,
      createdAt: new Date('2024-01-20')
    });
  }

  toggleEditMode(): void {
    this.isEditMode = true;
    this.entity$.subscribe(entity => {
      this.editableEntity = { ...entity };
    });
  }

  saveEntity(): void {
    if (this.editableEntity) {
      // Save entity logic here
      console.log('Saving entity:', this.editableEntity);
      this.isEditMode = false;
    }
  }

  cancelEdit(): void {
    this.isEditMode = false;
    this.editableEntity = null;
  }

  refreshEntityValues(): void {
    // Refresh entity values
    this.entityValues$ = of(this.getMockEntityValues());
  }

  private getMockTemplateRevisions(): TemplateRevision[] {
    return [
      {
        templateRevisionNumber: 3,
        templateRevisionDescription: 'Added new dependency columns',
        isActive: true,
        createdAt: new Date('2024-02-15'),
        createdBy: 'Admin'
      },
      {
        templateRevisionNumber: 2,
        templateRevisionDescription: 'Updated column order',
        isActive: false,
        createdAt: new Date('2024-02-01'),
        createdBy: 'Admin'
      },
      {
        templateRevisionNumber: 1,
        templateRevisionDescription: 'Initial template',
        isActive: false,
        createdAt: new Date('2024-01-20'),
        createdBy: 'System'
      }
    ];
  }

  private getMockEntityValues(): EntityValue[] {
    return [
      { entityValueId: 1, entityObjValue: 'Motor Type A', isActive: true, createdAt: new Date('2024-01-20') },
      { entityValueId: 2, entityObjValue: 'Motor Type B', isActive: true, createdAt: new Date('2024-01-21') },
      { entityValueId: 3, entityObjValue: 'Motor Type C', isActive: true, createdAt: new Date('2024-01-22') },
      { entityValueId: 4, entityObjValue: 'Motor Type D', isActive: false, createdAt: new Date('2024-01-23') },
      { entityValueId: 5, entityObjValue: 'Motor Type E', isActive: true, createdAt: new Date('2024-01-24') }
    ];
  }

  private getMockDataRevisions(): any[] {
    return [
      {
        dataRevisionNumber: 5,
        dataRevisionDescription: 'February data update',
        rowCount: 1250,
        isActive: true,
        createdAt: new Date('2024-02-28'),
        createdBy: 'John Doe'
      },
      {
        dataRevisionNumber: 4,
        dataRevisionDescription: 'Added new motor types',
        rowCount: 1200,
        isActive: false,
        createdAt: new Date('2024-02-15'),
        createdBy: 'Jane Smith'
      }
    ];
  }
}