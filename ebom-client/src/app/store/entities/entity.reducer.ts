import { createReducer, on } from '@ngrx/store';
import { initialEntityState } from './entity.models';
import * as EntityActions from './entity.actions';

export const entityReducer = createReducer(
  initialEntityState,
  
  // Load Entities
  on(EntityActions.loadEntities, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  on(EntityActions.loadEntitiesSuccess, (state, { entities }) => ({
    ...state,
    entities,
    loading: false,
    error: null
  })),
  on(EntityActions.loadEntitiesFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Load Entity
  on(EntityActions.loadEntity, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  on(EntityActions.loadEntitySuccess, (state, { entity }) => ({
    ...state,
    selectedEntity: entity,
    loading: false,
    error: null
  })),
  on(EntityActions.loadEntityFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Select Entity
  on(EntityActions.selectEntity, (state, { entity }) => ({
    ...state,
    selectedEntity: entity
  })),

  // Filter Actions
  on(EntityActions.setEntityTypeFilter, (state, { entityType }) => ({
    ...state,
    filters: {
      ...state.filters,
      entityType
    }
  })),
  on(EntityActions.setSearchFilter, (state, { searchTerm }) => ({
    ...state,
    filters: {
      ...state.filters,
      searchTerm
    }
  })),
  on(EntityActions.clearFilters, (state) => ({
    ...state,
    filters: {
      entityType: null,
      searchTerm: null
    }
  })),

  // Create Entity
  on(EntityActions.createEntity, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  on(EntityActions.createEntitySuccess, (state, { entity }) => ({
    ...state,
    entities: [...state.entities, entity],
    loading: false,
    error: null
  })),
  on(EntityActions.createEntityFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Update Entity
  on(EntityActions.updateEntity, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  on(EntityActions.updateEntitySuccess, (state, { entity }) => ({
    ...state,
    entities: state.entities.map(e => e.entityID === entity.entityID ? entity : e),
    selectedEntity: state.selectedEntity?.entityID === entity.entityID ? entity : state.selectedEntity,
    loading: false,
    error: null
  })),
  on(EntityActions.updateEntityFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  })),

  // Delete Entity
  on(EntityActions.deleteEntity, (state) => ({
    ...state,
    loading: true,
    error: null
  })),
  on(EntityActions.deleteEntitySuccess, (state, { id }) => ({
    ...state,
    entities: state.entities.filter(e => e.entityID !== id),
    selectedEntity: state.selectedEntity?.entityID === id ? null : state.selectedEntity,
    loading: false,
    error: null
  })),
  on(EntityActions.deleteEntityFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  }))
);