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
  }
]);

createRoot(document.getElementById('root')!).render(
  <RouterProvider router={router} />
)