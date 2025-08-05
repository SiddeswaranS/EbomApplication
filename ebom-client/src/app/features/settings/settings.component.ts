import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="p-6">
      <h1 class="text-2xl font-bold">Settings</h1>
      <p class="mt-4">Settings component - Coming soon</p>
    </div>
  `
})
export class SettingsComponent {}