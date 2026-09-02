import {
  postApiAuthForgotPassword,
  postApiAuthResetPassword,
} from '~/generated/api/auth/auth';

export function useAuthApi() {
  return {
    forgotPassword: postApiAuthForgotPassword,
    resetPassword: postApiAuthResetPassword,
  };
}
