import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DashboardData {
  uploadActivity: any[];
  entityDistribution: any[];
  recentActivities: any[];
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getDashboardData(): Observable<DashboardData> {
    // For now, return mock data. Replace with actual API call later
    const mockData: DashboardData = {
      uploadActivity: this.generateUploadActivity(),
      entityDistribution: [
        { type: 'ISR (Input Selection)', count: 45 },
        { type: 'PSR (Part Selection)', count: 68 },
        { type: 'CSR (Cost Selection)', count: 32 },
        { type: 'CMN (Common)', count: 11 }
      ],
      recentActivities: [
        {
          timestamp: new Date(Date.now() - 1000 * 60 * 5),
          action: 'Template Upload',
          entity: 'PSR_MotorSelection',
          user: 'John Doe',
          status: 'Success'
        },
        {
          timestamp: new Date(Date.now() - 1000 * 60 * 15),
          action: 'Data Upload',
          entity: 'ISR_BuildingType',
          user: 'Jane Smith',
          status: 'Success'
        },
        {
          timestamp: new Date(Date.now() - 1000 * 60 * 30),
          action: 'Template Update',
          entity: 'CSR_PricingRules',
          user: 'Mike Johnson',
          status: 'Processing'
        },
        {
          timestamp: new Date(Date.now() - 1000 * 60 * 45),
          action: 'Data Upload',
          entity: 'PSR_MotorSelection',
          user: 'Sarah Wilson',
          status: 'Failed'
        },
        {
          timestamp: new Date(Date.now() - 1000 * 60 * 60),
          action: 'Entity Creation',
          entity: 'CMN_Status',
          user: 'Admin',
          status: 'Success'
        }
      ]
    };

    return of(mockData);
    // Uncomment when API is ready:
    // return this.http.get<DashboardData>(`${this.apiUrl}/api/dashboard`);
  }

  private generateUploadActivity(): any[] {
    const days = 7;
    const data = [];
    const today = new Date();

    for (let i = days - 1; i >= 0; i--) {
      const date = new Date(today);
      date.setDate(today.getDate() - i);
      
      data.push({
        date: date,
        templates: Math.floor(Math.random() * 10) + 1,
        data: Math.floor(Math.random() * 20) + 5
      });
    }

    return data;
  }
}