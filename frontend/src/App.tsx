import { Route, Routes } from "react-router-dom";
import Layout from "./components/Layout";
import ProtectedRoute from "./components/ProtectedRoute";
import DashboardPage from "./pages/DashboardPage";
import PatientsPage from "./pages/PatientsPage";
import PatientDetailPage from "./pages/PatientDetailPage";
import DonorsPage from "./pages/DonorsPage";
import SessionsPage from "./pages/SessionsPage";
import CryoGridPage from "./pages/CryoGridPage";
import BagsPage from "./pages/BagsPage";
import BagDetailPage from "./pages/BagDetailPage";
import MovementsPage from "./pages/MovementsPage";
import InventoryPage from "./pages/InventoryPage";
import DliPage from "./pages/DliPage";
import LoginPage from "./pages/LoginPage";
import AdminRoute from "./components/AdminRoute";
import PlcIntegrationPage from "./pages/PlcIntegrationPage";
import ClinicalSettingsPage from "./pages/ClinicalSettingsPage";

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route
        element={
          <ProtectedRoute>
            <Layout />
          </ProtectedRoute>
        }
      >
        <Route index element={<DashboardPage />} />
        <Route path="/patients" element={<PatientsPage />} />
        <Route path="/patients/:id" element={<PatientDetailPage />} />
        <Route path="/donors" element={<DonorsPage />} />
        <Route path="/sessions" element={<SessionsPage />} />
        <Route path="/cryo" element={<CryoGridPage />} />
        <Route path="/bags" element={<BagsPage />} />
        <Route path="/bags/:id" element={<BagDetailPage />} />
        <Route path="/movements" element={<MovementsPage />} />
        <Route path="/dli" element={<DliPage />} />
        <Route path="/inventory" element={<InventoryPage />} />
        <Route
          path="/plc"
          element={
            <AdminRoute>
              <PlcIntegrationPage />
            </AdminRoute>
          }
        />
        <Route
          path="/clinical-settings"
          element={
            <AdminRoute>
              <ClinicalSettingsPage />
            </AdminRoute>
          }
        />
      </Route>
    </Routes>
  );
}
