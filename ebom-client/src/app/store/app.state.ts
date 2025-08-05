import { EntityState } from './entities/entity.models';

export interface AppState {
  entities: EntityState;
}

export const appReducers = {
  entities: () => import('./entities/entity.reducer').then(m => m.entityReducer)
};