import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { HttpClient } from '@angular/common/http';
import { of } from 'rxjs';
import { map, mergeMap, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Entity } from './entity.models';
import * as EntityActions from './entity.actions';

@Injectable()
export class EntityEffects {
  constructor(
    private actions$: Actions,
    private http: HttpClient
  ) {}

  loadEntities$ = createEffect(() =>
    this.actions$.pipe(
      ofType(EntityActions.loadEntities),
      mergeMap(() =>
        this.http.get<Entity[]>(`${environment.apiUrl}/api/entities`).pipe(
          map(entities => EntityActions.loadEntitiesSuccess({ entities })),
          catchError(error => of(EntityActions.loadEntitiesFailure({ 
            error: error.message || 'Failed to load entities' 
          })))
        )
      )
    )
  );

  loadEntity$ = createEffect(() =>
    this.actions$.pipe(
      ofType(EntityActions.loadEntity),
      mergeMap(action =>
        this.http.get<Entity>(`${environment.apiUrl}/api/entities/${action.id}`).pipe(
          map(entity => EntityActions.loadEntitySuccess({ entity })),
          catchError(error => of(EntityActions.loadEntityFailure({ 
            error: error.message || 'Failed to load entity' 
          })))
        )
      )
    )
  );

  createEntity$ = createEffect(() =>
    this.actions$.pipe(
      ofType(EntityActions.createEntity),
      mergeMap(action =>
        this.http.post<Entity>(`${environment.apiUrl}/api/entities`, action.entity).pipe(
          map(entity => EntityActions.createEntitySuccess({ entity })),
          catchError(error => of(EntityActions.createEntityFailure({ 
            error: error.message || 'Failed to create entity' 
          })))
        )
      )
    )
  );

  updateEntity$ = createEffect(() =>
    this.actions$.pipe(
      ofType(EntityActions.updateEntity),
      mergeMap(action =>
        this.http.put<Entity>(`${environment.apiUrl}/api/entities/${action.id}`, action.changes).pipe(
          map(entity => EntityActions.updateEntitySuccess({ entity })),
          catchError(error => of(EntityActions.updateEntityFailure({ 
            error: error.message || 'Failed to update entity' 
          })))
        )
      )
    )
  );

  deleteEntity$ = createEffect(() =>
    this.actions$.pipe(
      ofType(EntityActions.deleteEntity),
      mergeMap(action =>
        this.http.delete(`${environment.apiUrl}/api/entities/${action.id}`).pipe(
          map(() => EntityActions.deleteEntitySuccess({ id: action.id })),
          catchError(error => of(EntityActions.deleteEntityFailure({ 
            error: error.message || 'Failed to delete entity' 
          })))
        )
      )
    )
  );
}