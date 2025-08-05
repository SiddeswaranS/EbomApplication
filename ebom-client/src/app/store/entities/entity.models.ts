export interface Entity {
  entityID: number;
  entityName: string;
  entityDisplayName: string;
  entityType: string;
  dataTypeID: number;
  hasActiveTemplate: boolean;
  createdBy: number;
  createdDate: string;
  updatedBy?: number;
  updatedDate?: string;
}

export interface EntityState {
  entities: Entity[];
  selectedEntity: Entity | null;
  loading: boolean;
  error: string | null;
  filters: {
    entityType: string | null;
    searchTerm: string | null;
  };
}

export const initialEntityState: EntityState = {
  entities: [],
  selectedEntity: null,
  loading: false,
  error: null,
  filters: {
    entityType: null,
    searchTerm: null
  }
};