import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const authGuard: CanActivateFn = () => {
  // Inject the Router to enable programmatic navigation.
  const router = inject(Router);

  // Check whether a JWT token is stored in the browser's localStorage.
  const token = localStorage.getItem('jwt_token');

  // If the token exists, the user is authenticated and access is granted.
  if (token) {
    return true;
  }

  // Otherwise, redirect the user to the login page and block navigation.
  router.navigate(['/login']);
  return false;
};
