import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { 
  DxFileUploaderModule, 
  DxProgressBarModule, 
  DxAccordionModule, 
  DxDataGridModule,
  DxCheckBoxModule,
  DxLoadIndicatorModule,
  DxSelectBoxModule,
  DxButtonModule,
  DxTagBoxModule,
  DxPopupModule
} from 'devextreme-angular';
import { NotificationService } from '../../../core/services/notification.service';
import { environment } from '../../../../environments/environment';
import { HttpClient } from '@angular/common/http';

interface Entity {
  entityID: number;
  entityName: string;
  entityDisplayName: string;
  entityType: string;
  hasActiveTemplate: boolean;
}

interface DataUploadResult {
  isSuccess: boolean;
  fileName: string;
  entityId: number;
  dataRevisionId?: number;
  rowsProcessed?: number;
  rowsSkipped?: number;
  tableName?: string;
  sheetsProcessed?: string[];
  errors?: any[];
  warnings?: any[];
}

interface WizardStep {
  id: string;
  title: string;
  completed: boolean;
  active: boolean;
}

@Component({
  selector: 'app-data-upload-wizard',
  standalone: true,
  imports: [
    CommonModule,
    DxFileUploaderModule,
    DxProgressBarModule,
    DxAccordionModule,
    DxDataGridModule,
    DxCheckBoxModule,
    DxLoadIndicatorModule,
    DxSelectBoxModule,
    DxButtonModule,
    DxTagBoxModule,
    DxPopupModule
  ],
  template: `
    <div class="p-6">
      <h1 class="text-2xl font-bold mb-6">Data Upload Wizard</h1>

      <!-- Step Progress -->
      <div class="mb-8">
        <div class="flex items-center justify-between">
          <div *ngFor="let step of steps; let i = index" 
               class="flex items-center" 
               [class.flex-1]="i < steps.length - 1">
            
            <div class="flex items-center">
              <div class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium"
                   [class]="getStepClass(step)">
                {{ step.completed ? '✓' : (i + 1) }}
              </div>
              <span class="ml-2 text-sm font-medium" [class.text-gray-500]="!step.active && !step.completed">
                {{ step.title }}
              </span>
            </div>
            
            <div *ngIf="i < steps.length - 1" 
                 class="flex-1 h-0.5 mx-4"
                 [class.bg-blue-600]="step.completed"
                 [class.bg-gray-300]="!step.completed">
            </div>
          </div>
        </div>
      </div>

      <!-- Step 1: Entity Selection -->
      <div *ngIf="currentStep === 'entity'" class="bg-white rounded-lg shadow p-6">
        <h2 class="text-xl font-semibold mb-4">Select Entity</h2>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">Entity Type</label>
            <dx-select-box
              [dataSource]="entityTypes"
              valueExpr="value"
              displayExpr="text"
              [(value)]="selectedEntityType"
              (onValueChanged)="onEntityTypeChange($event)"
              placeholder="Select entity type">
            </dx-select-box>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">Entity</label>
            <dx-select-box
              [dataSource]="filteredEntities"
              valueExpr="entityID"
              displayExpr="entityDisplayName"
              [(value)]="selectedEntityId"
              [disabled]="!selectedEntityType"
              (onValueChanged)="onEntityChange($event)"
              placeholder="Select entity">
            </dx-select-box>
          </div>
        </div>

        <div *ngIf="selectedEntity" class="mt-6 p-4 bg-blue-50 rounded-lg">
          <h3 class="font-semibold text-blue-900 mb-2">Selected Entity Information:</h3>
          <div class="text-blue-800 space-y-1">
            <p><strong>Name:</strong> {{ selectedEntity.entityDisplayName }}</p>
            <p><strong>Type:</strong> {{ selectedEntity.entityType }}</p>
            <p><strong>Has Active Template:</strong> 
              <span [class]="selectedEntity.hasActiveTemplate ? 'text-green-600' : 'text-red-600'">
                {{ selectedEntity.hasActiveTemplate ? 'Yes' : 'No' }}
              </span>
            </p>
          </div>
          
          <div *ngIf="!selectedEntity.hasActiveTemplate" class="mt-2 p-2 bg-red-100 border border-red-300 rounded">
            <p class="text-red-800 text-sm">
              ⚠️ This entity does not have an active template. Please upload a template first.
            </p>
          </div>
        </div>

        <div class="mt-6 flex justify-end">
          <dx-button
            text="Next"
            type="default"
            stylingMode="contained"
            [disabled]="!selectedEntity || !selectedEntity.hasActiveTemplate"
            (onClick)="nextStep()">
          </dx-button>
        </div>
      </div>

      <!-- Step 2: File Upload -->
      <div *ngIf="currentStep === 'upload'" class="bg-white rounded-lg shadow p-6">
        <h2 class="text-xl font-semibold mb-4">Upload Data File</h2>

        <dx-file-uploader
          #uploader
          [multiple]="false"
          accept=".xlsx,.xls"
          uploadMode="instantly"
          [uploadUrl]="uploadUrl"
          [uploadHeaders]="uploadHeaders"
          (onUploaded)="onUploaded($event)"
          (onProgress)="onProgress($event)"
          (onUploadError)="onUploadError($event)"
          (onValueChanged)="onFileSelected($event)"
          [showFileList]="false">
        </dx-file-uploader>

        <div class="mt-6 p-4 bg-blue-50 rounded-lg">
          <h3 class="font-semibold text-blue-900 mb-2">Data Upload Guidelines:</h3>
          <ul class="list-disc list-inside text-blue-800 space-y-1">
            <li>File must be in Excel format (.xlsx or .xls)</li>
            <li>Data should be in sheets named Data01of01, Data02of02, etc.</li>
            <li>First row should contain column headers matching template</li>
            <li>Empty rows will be skipped</li>
            <li>Maximum file size: 50MB</li>
          </ul>
        </div>

        <!-- File Preview -->
        <div *ngIf="selectedFile && !uploadStarted" class="mt-6 p-4 border rounded-lg">
          <h4 class="font-semibold mb-2">Selected File:</h4>
          <p><strong>Name:</strong> {{ selectedFile.name }}</p>
          <p><strong>Size:</strong> {{ formatFileSize(selectedFile.size) }}</p>
          <p><strong>Entity:</strong> {{ selectedEntity?.entityDisplayName }}</p>
        </div>

        <!-- Upload Progress -->
        <div *ngIf="uploadProgress > 0 && uploadProgress < 100" class="mt-6">
          <dx-progress-bar
            [value]="uploadProgress"
            [showStatus]="true"
            [statusFormat]="formatProgress">
          </dx-progress-bar>
        </div>

        <!-- Processing Status -->
        <div *ngIf="processingStatus" class="mt-6">
          <div class="border rounded-lg p-4 bg-blue-50">
            <h4 class="font-semibold mb-2">{{ processingStatus.title }}</h4>
            <div class="space-y-2">
              <div *ngFor="let step of processingStatus.steps" class="flex items-center">
                <dx-check-box
                  [value]="step.completed"
                  [readOnly]="true"
                  class="mr-2">
                </dx-check-box>
                <span [class.line-through]="step.completed" [class.text-gray-500]="step.completed">
                  {{ step.name }}
                </span>
                <dx-load-indicator
                  *ngIf="step.inProgress"
                  [visible]="true"
                  class="ml-2"
                  [height]="20"
                  [width]="20">
                </dx-load-indicator>
              </div>
            </div>
          </div>
        </div>

        <div class="mt-6 flex justify-between">
          <dx-button
            text="Back"
            type="normal"
            stylingMode="outlined"
            (onClick)="previousStep()">
          </dx-button>
          <dx-button
            text="Next"
            type="default"
            stylingMode="contained"
            [disabled]="!uploadResult || !uploadResult.isSuccess"
            (onClick)="nextStep()">
          </dx-button>
        </div>
      </div>

      <!-- Step 3: Results -->
      <div *ngIf="currentStep === 'results'" class="bg-white rounded-lg shadow p-6">
        <h2 class="text-xl font-semibold mb-4">Upload Results</h2>

        <div *ngIf="uploadResult">
          <dx-accordion
            [collapsible]="true"
            [multiple]="true"
            [animationDuration]="300">
            
            <dxi-item title="Upload Summary" [expanded]="true">
              <div class="p-4 space-y-2">
                <p><strong>Status:</strong> 
                  <span [class]="uploadResult.isSuccess ? 'text-green-600' : 'text-red-600'">
                    {{ uploadResult.isSuccess ? 'Success' : 'Failed' }}
                  </span>
                </p>
                <p><strong>File:</strong> {{ uploadResult.fileName }}</p>
                <p><strong>Entity:</strong> {{ selectedEntity?.entityDisplayName }}</p>
                <p *ngIf="uploadResult.dataRevisionId">
                  <strong>Data Revision:</strong> {{ uploadResult.dataRevisionId }}
                </p>
                <p *ngIf="uploadResult.rowsProcessed">
                  <strong>Rows Processed:</strong> {{ uploadResult.rowsProcessed }}
                </p>
                <p *ngIf="uploadResult.rowsSkipped">
                  <strong>Rows Skipped:</strong> {{ uploadResult.rowsSkipped }}
                </p>
                <p *ngIf="uploadResult.tableName">
                  <strong>Table Created:</strong> {{ uploadResult.tableName }}
                </p>
              </div>
            </dxi-item>
            
            <dxi-item title="Sheets Processed" *ngIf="uploadResult.sheetsProcessed && uploadResult.sheetsProcessed.length > 0">
              <div class="p-4">
                <dx-tag-box
                  [dataSource]="uploadResult.sheetsProcessed"
                  [readOnly]="true"
                  [showSelectionControls]="false">
                </dx-tag-box>
              </div>
            </dxi-item>

            <dxi-item title="Errors" *ngIf="uploadResult.errors && uploadResult.errors.length > 0">
              <div class="p-4">
                <div *ngFor="let error of uploadResult.errors" 
                  class="mb-2 p-2 bg-red-50 border border-red-200 rounded">
                  <p class="text-red-800">{{ error.message }}</p>
                  <p class="text-sm text-red-600" *ngIf="error.details">{{ error.details }}</p>
                </div>
              </div>
            </dxi-item>

            <dxi-item title="Warnings" *ngIf="uploadResult.warnings && uploadResult.warnings.length > 0">
              <div class="p-4">
                <div *ngFor="let warning of uploadResult.warnings" 
                  class="mb-2 p-2 bg-yellow-50 border border-yellow-200 rounded">
                  <p class="text-yellow-800">{{ warning.message }}</p>
                  <p class="text-sm text-yellow-600" *ngIf="warning.suggestion">{{ warning.suggestion }}</p>
                </div>
              </div>
            </dxi-item>
          </dx-accordion>

          <div class="mt-6 flex justify-between">
            <div class="flex gap-4">
              <dx-button
                text="Upload Another"
                type="normal"
                stylingMode="outlined"
                (onClick)="resetWizard()">
              </dx-button>
              <dx-button
                *ngIf="uploadResult.isSuccess && selectedEntity"
                text="View Entity Data"
                type="default"
                stylingMode="contained"
                (onClick)="viewEntityData()">
              </dx-button>
            </div>
            <dx-button
              text="Finish"
              type="default"
              stylingMode="contained"
              (onClick)="finish()">
            </dx-button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }

    ::ng-deep .dx-fileuploader-input-wrapper {
      border: 2px dashed #d1d5db;
      border-radius: 0.5rem;
      padding: 2rem;
      text-align: center;
      cursor: pointer;
      transition: all 0.3s;
    }

    ::ng-deep .dx-fileuploader-input-wrapper:hover {
      border-color: #3b82f6;
      background-color: #f0f9ff;
    }
  `]
})
export class DataUploadWizardComponent implements OnInit {
  steps: WizardStep[] = [
    { id: 'entity', title: 'Select Entity', completed: false, active: true },
    { id: 'upload', title: 'Upload File', completed: false, active: false },
    { id: 'results', title: 'Results', completed: false, active: false }
  ];

  currentStep = 'entity';
  
  entityTypes = [
    { value: 'ISR', text: 'ISR - Installation Specific Requirements' },
    { value: 'PSR', text: 'PSR - Product Specific Requirements' },
    { value: 'CSR', text: 'CSR - Customer Specific Requirements' },
    { value: 'CMN', text: 'CMN - Common Requirements' }
  ];

  entities: Entity[] = [];
  filteredEntities: Entity[] = [];
  selectedEntityType: string = '';
  selectedEntityId: number | null = null;
  selectedEntity: Entity | null = null;
  
  selectedFile: File | null = null;
  uploadProgress = 0;
  uploadStarted = false;
  processingStatus: any;
  uploadResult: DataUploadResult | null = null;

  uploadUrl = '';
  uploadHeaders = {
    Authorization: `Bearer ${localStorage.getItem('access_token')}`
  };

  processingSteps = [
    { name: 'Validating file format', completed: false, inProgress: false },
    { name: 'Extracting data sheets', completed: false, inProgress: false },
    { name: 'Validating against template', completed: false, inProgress: false },
    { name: 'Creating data revision', completed: false, inProgress: false },
    { name: 'Inserting data records', completed: false, inProgress: false },
    { name: 'Finalizing upload', completed: false, inProgress: false }
  ];

  constructor(
    private notificationService: NotificationService,
    private router: Router,
    private route: ActivatedRoute,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.loadEntities();
    
    // Check if entity ID is provided in route params
    this.route.params.subscribe(params => {
      if (params['entityId']) {
        this.selectedEntityId = +params['entityId'];
        this.preSelectEntity();
      }
    });
  }

  async loadEntities(): Promise<void> {
    try {
      const response = await this.http.get<Entity[]>(`${environment.apiUrl}/api/entities`).toPromise();
      this.entities = response || [];
      this.filterEntitiesByType();
    } catch (error) {
      this.notificationService.error('Failed to load entities');
      console.error('Error loading entities:', error);
    }
  }

  preSelectEntity(): void {
    if (this.selectedEntityId && this.entities.length > 0) {
      const entity = this.entities.find(e => e.entityID === this.selectedEntityId);
      if (entity) {
        this.selectedEntityType = entity.entityType;
        this.selectedEntity = entity;
        this.filterEntitiesByType();
        this.updateUploadUrl();
      }
    }
  }

  onEntityTypeChange(e: any): void {
    this.selectedEntityType = e.value;
    this.selectedEntityId = null;
    this.selectedEntity = null;
    this.filterEntitiesByType();
  }

  filterEntitiesByType(): void {
    if (this.selectedEntityType) {
      this.filteredEntities = this.entities.filter(e => e.entityType === this.selectedEntityType);
    } else {
      this.filteredEntities = [];
    }
  }

  onEntityChange(e: any): void {
    this.selectedEntityId = e.value;
    this.selectedEntity = this.entities.find(entity => entity.entityID === this.selectedEntityId) || null;
    this.updateUploadUrl();
  }

  updateUploadUrl(): void {
    if (this.selectedEntityId) {
      this.uploadUrl = `${environment.apiUrl}/api/data/upload/${this.selectedEntityId}`;
    }
  }

  onFileSelected(e: any): void {
    if (e.value && e.value.length > 0) {
      this.selectedFile = e.value[0];
      this.uploadStarted = false;
      this.uploadResult = null;
      this.processingStatus = null;
    }
  }

  onUploaded(e: any): void {
    this.uploadStarted = false;
    
    if (e.request.status === 200) {
      const response = JSON.parse(e.request.response);
      this.uploadResult = response;
      
      if (response.isSuccess) {
        this.notificationService.success('Data uploaded successfully');
        this.completeStep('upload');
        this.activateStep('results');
      } else {
        this.notificationService.error('Data upload failed');
      }
    }
  }

  onProgress(e: any): void {
    this.uploadProgress = e.percentComplete;
    
    if (this.uploadProgress === 100 && !this.processingStatus) {
      this.uploadStarted = true;
      this.startProcessingAnimation();
    }
  }

  onUploadError(e: any): void {
    this.uploadStarted = false;
    this.notificationService.error('Upload failed: ' + (e.error || 'Unknown error'));
    this.uploadResult = {
      isSuccess: false,
      fileName: this.selectedFile?.name || '',
      entityId: this.selectedEntityId || 0,
      errors: [{ message: e.error || 'Upload failed' }]
    };
  }

  startProcessingAnimation(): void {
    this.processingStatus = {
      title: 'Processing Data...',
      steps: this.processingSteps.map(s => ({ ...s, completed: false, inProgress: false }))
    };

    this.processingSteps.forEach((step, index) => {
      setTimeout(() => {
        if (index > 0) {
          this.processingStatus.steps[index - 1].completed = true;
          this.processingStatus.steps[index - 1].inProgress = false;
        }
        if (index < this.processingSteps.length) {
          this.processingStatus.steps[index].inProgress = true;
        }
      }, index * 1000);
    });

    setTimeout(() => {
      this.processingStatus.steps.forEach((step: any) => {
        step.completed = true;
        step.inProgress = false;
      });
    }, this.processingSteps.length * 1000);
  }

  nextStep(): void {
    const currentIndex = this.steps.findIndex(s => s.id === this.currentStep);
    if (currentIndex < this.steps.length - 1) {
      this.completeStep(this.currentStep);
      this.currentStep = this.steps[currentIndex + 1].id;
      this.activateStep(this.currentStep);
    }
  }

  previousStep(): void {
    const currentIndex = this.steps.findIndex(s => s.id === this.currentStep);
    if (currentIndex > 0) {
      this.currentStep = this.steps[currentIndex - 1].id;
      this.activateStep(this.currentStep);
    }
  }

  completeStep(stepId: string): void {
    const step = this.steps.find(s => s.id === stepId);
    if (step) {
      step.completed = true;
      step.active = false;
    }
  }

  activateStep(stepId: string): void {
    this.steps.forEach(s => s.active = s.id === stepId);
  }

  getStepClass(step: WizardStep): string {
    if (step.completed) {
      return 'bg-green-600 text-white';
    } else if (step.active) {
      return 'bg-blue-600 text-white';
    } else {
      return 'bg-gray-300 text-gray-600';
    }
  }

  formatProgress = (ratio: number) => {
    return `Uploading: ${Math.round(ratio * 100)}%`;
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return Math.round(bytes / 1024) + ' KB';
    return Math.round(bytes / (1024 * 1024)) + ' MB';
  }

  resetWizard(): void {
    this.currentStep = 'entity';
    this.steps.forEach((step, index) => {
      step.completed = false;
      step.active = index === 0;
    });
    this.selectedFile = null;
    this.uploadProgress = 0;
    this.uploadStarted = false;
    this.processingStatus = null;
    this.uploadResult = null;
  }

  viewEntityData(): void {
    if (this.selectedEntity) {
      this.router.navigate(['/entities', this.selectedEntity.entityID]);
    }
  }

  finish(): void {
    this.router.navigate(['/entities']);
  }
}