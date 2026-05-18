SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

CREATE DATABASE IF NOT EXISTS `agencia_autos`;
USE `agencia_autos`;

CREATE TABLE `administrador` (
  `Id_Admin` int(11) NOT NULL,
  `Usuario` varchar(50) NOT NULL,
  `Password` varchar(255) NOT NULL,
  `Nombre` varchar(100) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `Rol` varchar(50) NOT NULL DEFAULT 'Estandar'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

INSERT INTO `administrador` (`Id_Admin`, `Usuario`, `Password`, `Nombre`, `Email`, `Rol`) VALUES
(1, 'admin', 'admin01', 'Administrador Principal', 'admin@agencia.com', 'Estandar');

CREATE TABLE `cliente` (
  `Id_Cliente` int(11) NOT NULL,
  `Nombre` varchar(100) NOT NULL,
  `Apellido` varchar(100) NOT NULL,
  `Telefono` varchar(15) NOT NULL,
  `Email` varchar(100) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

INSERT INTO `cliente` (`Id_Cliente`, `Nombre`, `Apellido`, `Telefono`, `Email`) VALUES
(1, 'Aldair', 'Lopez', '8445379136', 'aldairlopez@uadec.edu.mx'),
(2, 'Erik', 'Compian', '8448080876', 'erikc123@gmail.com'),
(3, 'Diego ', 'Gomez', '8445698978', 'diegogmz25@gmail.com'),
(4, 'Batorol', 'Garden', '8441955538', 'wallace_alfred@uadec.edu.mx');

CREATE TABLE `marca` (
  `Id_Marca` int(11) NOT NULL,
  `Nombre` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

INSERT INTO `marca` (`Id_Marca`, `Nombre`) VALUES
(5, 'Chevrolet'),
(3, 'Honda'),
(2, 'Nissan'),
(1, 'Toyota'),
(4, 'Volkswagen');

CREATE TABLE `modelo` (
  `Id_Modelo` int(11) NOT NULL,
  `Id_Marca` int(11) NOT NULL,
  `Nombre` varchar(50) NOT NULL,
  `Anio` year(4) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

INSERT INTO `modelo` (`Id_Modelo`, `Id_Marca`, `Nombre`, `Anio`) VALUES
(1, 3, 'Civic', '2010'),
(4, 5, 'Aveo', '2024'),
(5, 5, 'Silverado', '2023'),
(6, 3, 'CR-V', '2024'),
(7, 2, 'Sentra', '2024'),
(8, 2, 'Frontier', '2023'),
(9, 1, 'Corolla', '2024'),
(10, 1, 'Hilux', '2022'),
(11, 4, 'Jetta', '2024'),
(12, 4, 'Tiguan', '2024');

CREATE TABLE `proximo_servicio` (
  `Id_Prox_Serv` int(11) NOT NULL,
  `Folio` int(11) NOT NULL,
  `Fecha_Prog` date NOT NULL,
  `Km_Proximo` int(11) DEFAULT NULL,
  `Notas` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

INSERT INTO `proximo_servicio` (`Id_Prox_Serv`, `Folio`, `Fecha_Prog`, `Km_Proximo`, `Notas`) VALUES
(1, 1, '2026-10-25', 97000, 'Le toca cambio de bujías en 6 meses'),
(2, 3, '2026-10-25', 35000, 'Sistema: Recordatorio automático (6 meses o 10,000 km).'),
(3, 6, '2026-05-31', NULL, NULL);

CREATE TABLE `refaccion` (
  `Id_Refaccion` int(11) NOT NULL,
  `Nombre` varchar(100) NOT NULL,
  `Codigo` varchar(50) NOT NULL,
  `Precio` decimal(10,2) NOT NULL,
  `Stock` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

INSERT INTO `refaccion` (`Id_Refaccion`, `Nombre`, `Codigo`, `Precio`, `Stock`) VALUES
(1, 'Aceite Sintético 5W-30', 'ACE-001', 250.00, 48),
(2, 'Filtro de Aceite', 'FIL-002', 120.00, 30),
(3, 'Bujía Iridium', 'BUJ-003', 180.00, 95),
(4, 'Balatas Delanteras', 'BAL-004', 850.00, 19);

CREATE TABLE `servicio` (
  `Folio` int(11) NOT NULL,
  `Id_Vehiculo` int(11) NOT NULL,
  `Id_Admin` int(11) NOT NULL,
  `Id_Tipo_Serv` int(11) NOT NULL,
  `Quien_Entrego` varchar(100) NOT NULL,
  `Fecha_Ingreso` date NOT NULL,
  `Fecha_Salida` date DEFAULT NULL,
  `Estatus` enum('En espera','En proceso','Finalizado') NOT NULL DEFAULT 'En espera',
  `Descripcion` text DEFAULT NULL,
  `Fecha_Proximo_Servicio` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

INSERT INTO `servicio` (`Folio`, `Id_Vehiculo`, `Id_Admin`, `Id_Tipo_Serv`, `Quien_Entrego`, `Fecha_Ingreso`, `Fecha_Salida`, `Estatus`, `Descripcion`, `Fecha_Proximo_Servicio`) VALUES
(1, 1, 1, 1, 'William Wallace', '2026-04-25', '2026-04-25', 'Finalizado', 'Revisión general de rutina.', '2026-04-30 00:00:00'),
(2, 1, 1, 2, 'Aldair Lopez', '2026-04-25', NULL, 'En proceso', 'Requiere diagnóstico por falla.', '2026-04-30 00:00:00'),
(3, 2, 1, 1, 'Diego Gomez', '2026-04-25', '2026-04-25', 'Finalizado', 'Revisión general de rutina.', NULL),
(6, 1, 1, 1, 'El dueño', '2026-04-29', NULL, '', 'Mantenimiento Preventivo: Revisión de puntos de seguridad y niveles.', NULL),
(7, 3, 1, 1, 'Batorol Rosales', '2026-04-29', NULL, '', 'Mantenimiento Preventivo: Revisión de puntos de seguridad y niveles.', NULL);

CREATE TABLE `servicio_refaccion` (
  `Folio` int(11) NOT NULL,
  `Id_Refaccion` int(11) NOT NULL,
  `Cantidad` int(11) NOT NULL DEFAULT 1,
  `Precio_Aplicado` decimal(10,2) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

INSERT INTO `servicio_refaccion` (`Folio`, `Id_Refaccion`, `Cantidad`, `Precio_Aplicado`) VALUES
(1, 1, 1, 250.00),
(2, 1, 2, 250.00),
(6, 4, 1, 850.00),
(7, 3, 5, 900.00);

CREATE TABLE `tipo_servicio` (
  `Id_Tipo_Serv` int(11) NOT NULL,
  `Nombre` varchar(50) NOT NULL,
  `Descripcion` varchar(255) DEFAULT NULL,
  `Precio` decimal(10,2) NOT NULL DEFAULT 0.00
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

INSERT INTO `tipo_servicio` (`Id_Tipo_Serv`, `Nombre`, `Descripcion`, `Precio`) VALUES
(1, 'Preventivo', 'Mantenimiento programado para prevenir fallas', 850.00),
(2, 'Correctivo', 'Reparación de fallas existentes en el vehículo', 1250.00);

CREATE TABLE `usuarios` (
  `Id_Usuario` int(11) NOT NULL,
  `Nombre` varchar(100) NOT NULL,
  `Usuario` varchar(100) NOT NULL,
  `Password` varchar(100) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `Rol` varchar(50) NOT NULL DEFAULT 'Estandar'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

INSERT INTO `usuarios` (`Id_Usuario`, `Nombre`, `Usuario`, `Password`, `Email`, `Rol`) VALUES
(1, 'william wallace', 'william@gmail.com', 'user001', 'william@gmail.com', 'Estandar'),
(2, 'Walter Wilde', 'Walter', 'user002', 'Walter@gmail.com', 'Mecanico');

CREATE TABLE `vehiculo` (
  `Id_Vehiculo` int(11) NOT NULL,
  `Id_Cliente` int(11) NOT NULL,
  `Id_Modelo` int(11) NOT NULL,
  `Num_Serie` varchar(50) NOT NULL,
  `Color` varchar(30) DEFAULT NULL,
  `Placa` varchar(20) NOT NULL,
  `Km_Actual` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

INSERT INTO `vehiculo` (`Id_Vehiculo`, `Id_Cliente`, `Id_Modelo`, `Num_Serie`, `Color`, `Placa`, `Km_Actual`) VALUES
(1, 1, 1, '800142fg', 'Plata', 'FKD9156', 92000),
(2, 3, 4, '3G1BE51RS123456', 'Negro', 'MEX2026', 25000),
(3, 4, 10, '834142yj', 'Gris Rata', 'DER894', 11000);

ALTER TABLE `administrador`
  ADD PRIMARY KEY (`Id_Admin`),
  ADD UNIQUE KEY `Usuario` (`Usuario`),
  ADD UNIQUE KEY `Email` (`Email`);

ALTER TABLE `cliente`
  ADD PRIMARY KEY (`Id_Cliente`);

ALTER TABLE `marca`
  ADD PRIMARY KEY (`Id_Marca`),
  ADD UNIQUE KEY `Nombre` (`Nombre`);

ALTER TABLE `modelo`
  ADD PRIMARY KEY (`Id_Modelo`),
  ADD KEY `FK_Mod_Marca` (`Id_Marca`);

ALTER TABLE `proximo_servicio`
  ADD PRIMARY KEY (`Id_Prox_Serv`),
  ADD UNIQUE KEY `Folio` (`Folio`);

ALTER TABLE `refaccion`
  ADD PRIMARY KEY (`Id_Refaccion`),
  ADD UNIQUE KEY `Codigo` (`Codigo`);

ALTER TABLE `servicio`
  ADD PRIMARY KEY (`Folio`),
  ADD KEY `FK_Ser_Vehiculo` (`Id_Vehiculo`),
  ADD KEY `FK_Ser_Admin` (`Id_Admin`),
  ADD KEY `FK_Ser_TipoServ` (`Id_Tipo_Serv`);

ALTER TABLE `servicio_refaccion`
  ADD PRIMARY KEY (`Folio`,`Id_Refaccion`),
  ADD KEY `FK_SR_Refaccion` (`Id_Refaccion`);

ALTER TABLE `tipo_servicio`
  ADD PRIMARY KEY (`Id_Tipo_Serv`),
  ADD UNIQUE KEY `Nombre` (`Nombre`);

ALTER TABLE `usuarios`
  ADD PRIMARY KEY (`Id_Usuario`),
  ADD UNIQUE KEY `Usuario` (`Usuario`);

ALTER TABLE `vehiculo`
  ADD PRIMARY KEY (`Id_Vehiculo`),
  ADD UNIQUE KEY `Num_Serie` (`Num_Serie`),
  ADD UNIQUE KEY `Placa` (`Placa`),
  ADD KEY `FK_Veh_Cliente` (`Id_Cliente`),
  ADD KEY `FK_Veh_Modelo` (`Id_Modelo`);

ALTER TABLE `administrador`
  MODIFY `Id_Admin` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

ALTER TABLE `cliente`
  MODIFY `Id_Cliente` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

ALTER TABLE `marca`
  MODIFY `Id_Marca` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

ALTER TABLE `modelo`
  MODIFY `Id_Modelo` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=13;

ALTER TABLE `proximo_servicio`
  MODIFY `Id_Prox_Serv` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

ALTER TABLE `refaccion`
  MODIFY `Id_Refaccion` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

ALTER TABLE `servicio`
  MODIFY `Folio` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

ALTER TABLE `tipo_servicio`
  MODIFY `Id_Tipo_Serv` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

ALTER TABLE `usuarios`
  MODIFY `Id_Usuario` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

ALTER TABLE `vehiculo`
  MODIFY `Id_Vehiculo` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

ALTER TABLE `modelo`
  ADD CONSTRAINT `FK_Mod_Marca` FOREIGN KEY (`Id_Marca`) REFERENCES `marca` (`Id_Marca`) ON UPDATE CASCADE;

ALTER TABLE `proximo_servicio`
  ADD CONSTRAINT `FK_Prox_Servicio` FOREIGN KEY (`Folio`) REFERENCES `servicio` (`Folio`) ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `servicio`
  ADD CONSTRAINT `FK_Ser_Admin` FOREIGN KEY (`Id_Admin`) REFERENCES `administrador` (`Id_Admin`) ON UPDATE CASCADE,
  ADD CONSTRAINT `FK_Ser_TipoServ` FOREIGN KEY (`Id_Tipo_Serv`) REFERENCES `tipo_servicio` (`Id_Tipo_Serv`) ON UPDATE CASCADE,
  ADD CONSTRAINT `FK_Ser_Vehiculo` FOREIGN KEY (`Id_Vehiculo`) REFERENCES `vehiculo` (`Id_Vehiculo`) ON UPDATE CASCADE;

ALTER TABLE `servicio_refaccion`
  ADD CONSTRAINT `FK_SR_Refaccion` FOREIGN KEY (`Id_Refaccion`) REFERENCES `refaccion` (`Id_Refaccion`) ON UPDATE CASCADE,
  ADD CONSTRAINT `FK_SR_Servicio` FOREIGN KEY (`Folio`) REFERENCES `servicio` (`Folio`) ON DELETE CASCADE ON UPDATE CASCADE;

COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
