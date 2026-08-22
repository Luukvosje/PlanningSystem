import {
  getApiInvitesCode,
  postApiInvites,
  postApiInvitesAccept,
} from '~/generated/api/invites/invites';

export function useInvitesApi() {
  return {
    create: postApiInvites,
    accept: postApiInvitesAccept,
    preview: getApiInvitesCode,
  };
}
