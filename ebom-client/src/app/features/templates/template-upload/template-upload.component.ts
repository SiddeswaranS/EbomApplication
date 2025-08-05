import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DxFileUploaderModule, DxProgressBarModule, DxAccordionModule, DxDataGridModule, DxCheckBoxModule, DxLoadIndicatorModule } from 'devextreme-angular';
import { NotificationService } from '../../../core/services/notification.service';
import { environment } from '../../../../environments/environment';
import { HttpClient, HttpHeaders } from '@angular/common/http';

interface ProcessingStep {
  name: string;
  completed: boolean;
  inProgress: boolean;
}

interface UploadResult {
  isSuccess: boolean;
  fileName: string;
  entityType: string;
  entityName: string;
  entityId?: number;
  templateRevision?: number;
  columnsProcessed?: number;
  columnDetails?: any[];
  errors?: any[];
  warnings?: any[];
}

@Component({
  selector: 'app-template-upload',
  standalone: true,
  imports: [
    CommonModule,
    DxFileUploaderModule,
    DxProgressBarModule,
    DxAccordionModule,
    DxDataGridModule,
    DxCheckBoxModule,
    DxLoadIndicatorModule
  ],
  template: `
    <div class="p-6">
      <h1 class="text-2xl font-bold mb-6">Template Upload</h1>

      <div class="bg-white rounded-lg shadow p-6">
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
          <h3 class="font-semibold text-blue-900 mb-2">Template Upload Guidelines:</h3>
          <ul class="list-disc list-inside text-blue-800 space-y-1">
            <li>File must be in Excel format (.xlsx or .xls)</li>
            <li>File naming convention: [EntityType]_[EntityName].xlsx</li>
            <li>Entity types: ISR, PSR, CSR, CMN</li>
            <li>Must contain Configuration sheet and data sheets (Data01of01, etc.)</li>
            <li>Maximum file size: 50MB</li>
          </ul>
        </div>

        <!-- File Preview -->
        <div *ngIf="selectedFile && !uploadStarted" class="mt-6 p-4 border rounded-lg">
          <h4 class="font-semibold mb-2">Selected File:</h4>
          <p><strong>Name:</strong> {{ selectedFile.name }}</p>
          <p><strong>Size:</strong> {{ formatFileSize(selectedFile.size) }}</p>
          <div *ngIf="parsedFileInfo" class="mt-2">
            <p><strong>Entity Type:</strong> {{ parsedFileInfo.entityType }}</p>
            <p><strong>Entity Name:</strong> {{ parsedFileInfo.entityName }}</p>
          </div>
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
          <div class="border rounded-lg p-4" [ngClass]="getStatusClass()">
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

        <!-- Results -->
        <div *ngIf="uploadResult" class="mt-6">
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
                <p><strong>Entity Type:</strong> {{ uploadResult.entityType }}</p>
                <p><strong>Entity Name:</strong> {{ uploadResult.entityName }}</p>
                <p *ngIf="uploadResult.templateRevision">
                  <strong>Template Revision:</strong> {{ uploadResult.templateRevision }}
                </p>
                <p *ngIf="uploadResult.columnsProcessed">
                  <strong>Columns Processed:</strong> {{ uploadResult.columnsProcessed }}
                </p>
              </div>
            </dxi-item>
            
            <dxi-item title="Column Details" *ngIf="uploadResult.columnDetails && uploadResult.columnDetails.length > 0">
              <dx-data-grid
                [dataSource]="uploadResult.columnDetails"
                [showBorders]="true"
                [columnAutoWidth]="true">
                <dxi-column dataField="columnName" caption="Column Name"></dxi-column>
                <dxi-column dataField="columnType" caption="Type"></dxi-column>
                <dxi-column dataField="dataType" caption="Data Type"></dxi-column>
                <dxi-column dataField="isValueType" caption="Value Type" dataType="boolean"></dxi-column>
              </dx-data-grid>
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

          <div class="mt-6 flex justify-end gap-4">
            <button 
              class="px-4 py-2 bg-gray-200 text-gray-800 rounded hover:bg-gray-300"
              (click)="resetUpload()">
              Upload Another
            </button>
            <button 
              *ngIf="uploadResult.isSuccess && uploadResult.entityId"
              class="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
              (click)="viewEntity()">
              View Entity
            </button>
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

    ::ng-deep .dx-fileuploader-input-label {
      font-size: 1rem;
      color: #374151;
    }
  `]
})
export class TemplateUploadComponent implements OnInit {
  uploadUrl = `${environment.apiUrl}/api/templates/upload`;
  uploadHeaders = {
    Authorization: `Bearer ${localStorage.getItem('access_token')}`
  };
  
  uploadProgress = 0;
  uploadStarted = false;
  processingStatus: any;
  uploadResult: UploadResult | null = null;
  selectedFile: File | null = null;
  parsedFileInfo: { entityType: string; entityName: string } | null = null;

  processingSteps: ProcessingStep[] = [
    { name: 'Validating file format', completed: false, inProgress: false },
    { name: 'Extracting entity information', completed: false, inProgress: false },
    { name: 'Processing columns', completed: false, inProgress: false },
    { name: 'Creating template revision', completed: false, inProgress: false },
    { name: 'Processing mirror entities', completed: false, inProgress: false },
    { name: 'Finalizing upload', completed: false, inProgress: false }
  ];

  constructor(
    private notificationService: NotificationService,
    private router: Router,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    // Initialize component
  }

  onFileSelected(e: any): void {
    if (e.value && e.value.length > 0) {
      this.selectedFile = e.value[0];
      this.parseFileName(this.selectedFile!.name);
      this.uploadStarted = false;
      this.uploadResult = null;
      this.processingStatus = null;
    }
  }

  parseFileName(fileName: string): void {
    const nameWithoutExtension = fileName.replace(/\.[^/.]+$/, '');
    const parts = nameWithoutExtension.split('_');
    
    if (parts.length >= 2) {
      this.parsedFileInfo = {
        entityType: parts[0],
        entityName: parts.slice(1).join('_')
      };
    } else {
      this.parsedFileInfo = null;
    }
  }

  onUploaded(e: any): void {
    this.uploadStarted = false;
    
    if (e.request.status === 200) {
      const response = JSON.parse(e.request.response);
      this.uploadResult = response;
      
      if (response.isSuccess) {
        this.notificationService.success('Template uploaded successfully');
      } else {
        this.notificationService.error('Template upload failed');
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
      entityType: this.parsedFileInfo?.entityType || '',
      entityName: this.parsedFileInfo?.entityName || '',
      errors: [{ message: e.error || 'Upload failed' }]
    };
  }

  startProcessingAnimation(): void {
    this.processingStatus = {
      title: 'Processing Template...',
      steps: this.processingSteps.map(s => ({ ...s, completed: false, inProgress: false }))
    };

    // Simulate processing steps
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

    // Complete all steps after animation
    setTimeout(() => {
      this.processingStatus.steps.forEach((step: ProcessingStep) => {
        step.completed = true;
        step.inProgress = false;
      });
    }, this.processingSteps.length * 1000);
  }

  formatProgress = (ratio: number) => {
    return `Uploading: ${Math.round(ratio * 100)}%`;
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return Math.round(bytes / 1024) + ' KB';
    return Math.round(bytes / (1024 * 1024)) + ' MB';
  }

  getStatusClass(): string {
    if (this.uploadResult?.isSuccess) {
      return 'border-green-300 bg-green-50';
    } else if (this.uploadResult?.errors && this.uploadResult.errors.length > 0) {
      return 'border-red-300 bg-red-50';
    }
    return 'border-blue-300 bg-blue-50';
  }

  resetUpload(): void {
    this.selectedFile = null;
    this.parsedFileInfo = null;
    this.uploadProgress = 0;
    this.uploadStarted = false;
    this.processingStatus = null;
    this.uploadResult = null;
  }

  viewEntity(): void {
    if (this.uploadResult?.entityId) {
      this.router.navigate(['/entities', this.uploadResult.entityId]);
    }
  }
}