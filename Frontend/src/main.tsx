import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import {
  createBrowserRouter,
  RouterProvider,
  Route,
  Link,
} from 'react-router';

import MainPage from './pages/MainPage.tsx'
import OnBoardingMain from './pages/OnBoarding/OnBoardingMain.tsx';
import LoginPage from './pages/LoginPage.tsx';
import AddressPage from './pages/AddressPage.tsx';
import ProfilePage from './pages/ProfilePage.tsx';
import BasketPage from './pages/BasketPage.tsx';

const router = createBrowserRouter([
  {
      path: '/',
      element: (
          <MainPage/>
      ),
  },
  {
      path: '/onboarding',
      element: (
          <OnBoardingMain/>
      ),
  },
  {
      path: '/address_select',
      element: (
          <AddressPage/>
      ),
  },
  {
    path: '/basket',
    element: (
        <BasketPage/>
    ),
    },
  {
      path: '/login',
      element: (
          <LoginPage/>
      ),
  }
]);

createRoot(document.getElementById('root')!).render(
  <RouterProvider router={router} />
)