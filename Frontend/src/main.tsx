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
import OrderPage from './pages/OrderPage.tsx';
import OrderInfoPage from './pages/OrderInfoPage.tsx';
import PaymentPage from './pages/PaymentPage.tsx';
import FoodItemsPage from './pages/FoodItemsPage.tsx';
import CourierRequestPage from "./pages/RequestsPages/CourierRequestPage.tsx";
import RestaurantRequestPage from "./pages/RequestsPages/RestaurantRequestPage.tsx";
import RequestsPage from './pages/AdminPages/RequestsPage.tsx';
import UsersPage from './pages/AdminPages/UsersPage.tsx';
import RestaurantsOwnerPage from './pages/RestaurantPages/RestaurantsOwnerPage.tsx';
import RestaurantsOrdersOwnerPage from './pages/RestaurantPages/RestaurantsOrdersOwnerPage.tsx';

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
    path: '/ordered',
    element: (
        <OrderPage/>
    ),
  },
  {
    path: '/payment',
    element: (
        <PaymentPage/>
    ),
  },
  {
    path: '/ordersinfo',
    element: (
        <OrderInfoPage/>
    ),
  },
  {
    path: '/login',
    element: (
        <LoginPage/>
    ),
  },
  {
    path: '/fooditems',
    element: (
        <FoodItemsPage/>
    ),
  },
  {
    path: '/courier-request',
    element: (
        <CourierRequestPage/>
    ),
  },
  {
    path: '/restaurant-request',
    element: (
        <RestaurantRequestPage/>
    ),
  },
  {
    path: '/requests',
    element: (
        <RequestsPage/>
    ),
  },
  {
    path: '/users',
    element: (
        <UsersPage/>
    ),
  },
  {
    path: '/restaurantsOwner',
    element: (
        <RestaurantsOwnerPage/>
    ),
  },
  {
    path: '/restaurantsOwner-orders',
    element: (
        <RestaurantsOrdersOwnerPage/>
    ),
  },
]);

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <RouterProvider router={router} />
  </StrictMode>
)