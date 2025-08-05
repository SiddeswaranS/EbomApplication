import { createFeatureSelector, createSelector } from '@ngrx/store';
import { EntityState } from './entity.models';

export const selectEntityState = createFeatureSelector<EntityState>('entities');

export const selectAllEntities = createSelector(
  selectEntityState,
  (state) => state.entities
);

export const selectSelectedEntity = createSelector(
  selectEntityState,
  (state) => state.selectedEntity
);

export const selectEntityLoading = createSelector(
  selectEntityState,
  (state) => state.loading
);

export const selectEntityError = createSelector(
  selectEntityState,
  (state) => state.error
);

export const selectEntityFilters = createSelector(
  selectEntityState,
  (state) => state.filters
);

export const selectFilteredEntities = createSelector(
  selectAllEntities,
  selectEntityFilters,
  (entities, filters) => {
    let filteredEntities = entities;

    if (filters.entityType) {
      filteredEntities = filteredEntities.filter(e => e.entityType === filters.entityType);
    }

    if (filters.searchTerm) {
      const searchTerm = filters.searchTerm.toLowerCase();
      filteredEntities = filteredEntities.filter(e => 
        e.entityName.toLowerCase().includes(searchTerm) ||
        e.entityDisplayName.toLowerCase().includes(searchTerm)
      );
    }

    return filteredEntities;
  }
);

export const selectEntitiesByType = createSelector(
  selectAllEntities,
  (entities) => {
    const grouped = entities.reduce((acc, entity) => {
      if (!acc[entity.entityType]) {
        acc[entity.entityType] = [];
      }
      acc[entity.entityType].push(entity);
      return acc;
    }, {} as Record<string, typeof entities>);
    
    return grouped;
  }
);

export const selectEntityById = (id: number) => createSelector(
  selectAllEntities,
  (entities) => entities.find(entity => entity.entityID === id) || null
);

export const selectEntitiesWithActiveTemplates = createSelector(
  selectAllEntities,
  (entities) => entities.filter(e => e.hasActiveTemplate)
);

export const selectEntityStats = createSelector(
  selectAllEntities,
  (entities) => ({
    total: entities.length,
    withTemplates: entities.filter(e => e.hasActiveTemplate).length,
    byType: entities.reduce((acc, entity) => {
      acc[entity.entityType] = (acc[entity.entityType] || 0) + 1;
      return acc;
    }, {} as Record<string, number>)
  })
);