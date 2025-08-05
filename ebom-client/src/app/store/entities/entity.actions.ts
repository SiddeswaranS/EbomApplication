import { createAction, props } from '@ngrx/store';
import { Entity } from './entity.models';

// Load Entities
export const loadEntities = createAction('[Entity] Load Entities');
export const loadEntitiesSuccess = createAction(
  '[Entity] Load Entities Success',
  props<{ entities: Entity[] }>()
);
export const loadEntitiesFailure = createAction(
  '[Entity] Load Entities Failure',
  props<{ error: string }>()
);

// Load Entity by ID
export const loadEntity = createAction(
  '[Entity] Load Entity',
  props<{ id: number }>()
);
export const loadEntitySuccess = createAction(
  '[Entity] Load Entity Success',
  props<{ entity: Entity }>()
);
export const loadEntityFailure = createAction(
  '[Entity] Load Entity Failure',
  props<{ error: string }>()
);

// Select Entity
export const selectEntity = createAction(
  '[Entity] Select Entity',
  props<{ entity: Entity | null }>()
);

// Filter Entities
export const setEntityTypeFilter = createAction(
  '[Entity] Set Entity Type Filter',
  props<{ entityType: string | null }>()
);
export const setSearchFilter = createAction(
  '[Entity] Set Search Filter',
  props<{ searchTerm: string | null }>()
);
export const clearFilters = createAction('[Entity] Clear Filters');

// Create Entity
export const createEntity = createAction(
  '[Entity] Create Entity',
  props<{ entity: Omit<Entity, 'entityID' | 'createdDate' | 'createdBy'> }>()
);
export const createEntitySuccess = createAction(
  '[Entity] Create Entity Success',
  props<{ entity: Entity }>()
);
export const createEntityFailure = createAction(
  '[Entity] Create Entity Failure',
  props<{ error: string }>()
);

// Update Entity
export const updateEntity = createAction(
  '[Entity] Update Entity',
  props<{ id: number; changes: Partial<Entity> }>()
);
export const updateEntitySuccess = createAction(
  '[Entity] Update Entity Success',
  props<{ entity: Entity }>()
);
export const updateEntityFailure = createAction(
  '[Entity] Update Entity Failure',
  props<{ error: string }>()
);

// Delete Entity
export const deleteEntity = createAction(
  '[Entity] Delete Entity',
  props<{ id: number }>()
);
export const deleteEntitySuccess = createAction(
  '[Entity] Delete Entity Success',
  props<{ id: number }>()
);
export const deleteEntityFailure = createAction(
  '[Entity] Delete Entity Failure',
  props<{ error: string }>()
);