import {
  getApiAuthMe,
  postApiAuthLogin,
  postApiAuthRegister,
} from '~/generated/api/auth/auth'

export function useAuthApi() {
  return {
    register: postApiAuthRegister,
    login: postApiAuthLogin,
    me: getApiAuthMe,
  }
}
