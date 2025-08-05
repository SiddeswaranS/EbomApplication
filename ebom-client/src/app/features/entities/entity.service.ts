import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Entity } from './entity-list/entity-list.component';

@Injectable({ providedIn: 'root' })
export class EntityService {
  private apiUrl = `${environment.apiUrl}/api/entities`;

  constructor(private http: HttpClient) {}

  getEntities(): Observable<Entity[]> {
    // Mock data for now
    const mockEntities: Entity[] = [
      {
        entityID: 1,
        entityName: 'BuildingType',
        entityDisplayName: 'Building Type',
        entityType: 'ISR',
        dataType: 'String',
        isActive: true,
        createdAt: new Date('2024-01-15')
      },
      {
        entityID: 2,
        entityName: 'MotorSelection',
        entityDisplayName: 'Motor Selection',
        entityType: 'PSR',
        dataType: 'String',
        isActive: true,
        createdAt: new Date('2024-01-20')
      },
      {
        entityID: 3,
        entityName: 'PricingRules',
        entityDisplayName: 'Pricing Rules',
        entityType: 'CSR',
        dataType: 'Range',
        isActive: true,
        createdAt: new Date('2024-01-25')
      },
      {
        entityID: 4,
        entityName: 'Status',
        entityDisplayName: 'Status',
        entityType: 'CMN',
        dataType: 'String',
        isActive: true,
        createdAt: new Date('2024-02-01')
      },
      {
        entityID: 5,
        entityName: 'Capacity',
        entityDisplayName: 'Elevator Capacity',
        entityType: 'ISR',
        dataType: 'Integer',
        isActive: true,
        createdAt: new Date('2024-02-05')
      }
    ];

    return of(mockEntities);
    // Uncomment when API is ready:
    // return this.http.get<Entity[]>(this.apiUrl);
  }

  getEntity(id: number): Observable<Entity> {
    return this.http.get<Entity>(`${this.apiUrl}/${id}`);
  }

  createEntity(entity: Partial<Entity>): Observable<Entity> {
    return this.http.post<Entity>(this.apiUrl, entity);
  }

  updateEntity(id: number, entity: Partial<Entity>): Observable<Entity> {
    return this.http.put<Entity>(`${this.apiUrl}/${id}`, entity);
  }

  deleteEntity(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getEntityWithTemplates(): Observable<Entity[]> {
    return this.http.get<Entity[]>(`${this.apiUrl}/with-templates`);
  }
}