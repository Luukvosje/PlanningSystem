import {
  getApiOrganizationsCurrent,
  getApiOrganizationsId,
  getApiOrganizationsMine,
  postApiOrganizations,
} from '~/generated/api/organizations/organizations';

export function useOrganizationsApi() {
  return {
    create: postApiOrganizations,
    getCurrent: getApiOrganizationsCurrent,
    getMine: getApiOrganizationsMine,
    getById: getApiOrganizationsId,
  };
}
