import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-upload-wizard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="p-6">
      <h1 class="text-2xl font-bold">Data Upload</h1>
      <p class="mt-4">Data upload wizard - Coming soon</p>
    </div>
  `
})
export class UploadWizardComponent {}