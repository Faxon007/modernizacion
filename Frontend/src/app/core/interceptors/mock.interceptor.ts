import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { of } from 'rxjs';
import { delay } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export const mockInterceptor: HttpInterceptorFn = (req, next) => {
  // Si la bandera useDummyData está desactivada, pasar la solicitud al siguiente interceptor/backend
  if (!(environment as any).useDummyData) {
    return next(req);
  }

  const url = req.url;
  console.log(`[MockInterceptor] Interceptando llamada a: ${req.method} ${url}`);

  // Simulación de retraso de red
  const networkDelay = 400;

  // Helper para retornar respuestas JSON exitosas
  const jsonResponse = (data: any, status = 200) => {
    return of(new HttpResponse({ status, body: data })).pipe(delay(networkDelay));
  };

  // 1. Autenticación /auth/token
  if (url.includes('/auth/token')) {
    return jsonResponse({
      success: true,
      code: 'AUTH.SUCCESS',
      message: 'Autenticación simulada exitosa',
      data: {
        accessToken: 'mock-jwt-token-1234567890-abcdef',
        expiresAt: new Date(Date.now() + 8 * 3600 * 1000).toISOString(), // 8 horas
        username: (req.body as any)?.username || 'usuario.demo',
        role: 'ADMIN_PROMERICA'
      }
    });
  }

  // 2. Menú Dinámico /Menu/items
  if (url.includes('/Menu/items')) {
    return jsonResponse({
      success: true,
      code: 'MENU.SUCCESS',
      message: 'Menú cargado desde datos simulados',
      data: [
        {
          id: 1,
          nombre: 'Cobros Neo En Link',
          path: '',
          descripcion: 'Gestión de cobros y links de pago',
          padreId: 0,
          visible: true
        },
        {
          id: 2,
          nombre: 'Emisión de Link',
          path: 'frmEmisionLink.aspx',
          descripcion: 'Emitir un nuevo link de pago de VisaEnLink',
          padreId: 1,
          visible: true
        },
        {
          id: 3,
          nombre: 'Activación de Link',
          path: 'frmActivacion.aspx',
          descripcion: 'Activación y control manual de estados de links',
          padreId: 1,
          visible: true
        },
        {
          id: 4,
          nombre: 'Cancelar Link',
          path: 'frmCancelarLink.aspx',
          descripcion: 'Cancelar programaciones de pago o links emitidos',
          padreId: 1,
          visible: true
        },
        {
          id: 5,
          nombre: 'Carga Masiva',
          path: 'frmCargaMasiva.aspx',
          descripcion: 'Cargar lotes masivos de links de pago',
          padreId: 1,
          visible: true
        },
        {
          id: 6,
          nombre: 'Verificación de Links',
          path: 'frmVerificacionLink.aspx',
          descripcion: 'Verificación e historial de cobros procesados',
          padreId: 1,
          visible: true
        },
        {
          id: 7,
          nombre: 'Parámetros del Sistema',
          path: 'frmParametros.aspx',
          descripcion: 'Configurar notificaciones, montos e imagen publicitaria',
          padreId: 1,
          visible: true
        }
      ]
    });
  }

  // 3. Endpoints de Links
  if (url.includes('/links/get-links-verifica')) {
    return jsonResponse({
      draw: (req.body as any)?.draw || 1,
      recordsTotal: 3,
      recordsFiltered: 3,
      data: [
        {
          correlativo: '1',
          producto: 'Pago Mínimo Tarjeta Oro',
          codigoVisa: 'VISA-992211',
          numAuto: '102938',
          numMov: '5002010',
          edit: 'N'
        },
        {
          correlativo: '2',
          producto: 'Pago de Préstamo Auto',
          codigoVisa: 'VISA-883344',
          numAuto: '564738',
          numMov: '5002011',
          edit: 'S'
        },
        {
          correlativo: '3',
          producto: 'Colegiatura Universitaria',
          codigoVisa: 'VISA-774455',
          numAuto: '348712',
          numMov: '5002012',
          edit: 'S'
        }
      ]
    });
  }

  if (url.includes('/links/get-links')) {
    return jsonResponse({
      draw: (req.body as any)?.draw || 1,
      recordsTotal: 4,
      recordsFiltered: 4,
      data: [
        {
          numCuenta: '4019283746',
          tipCuenta: 'TC',
          monto: 350.00,
          tipPago: 'Contado',
          esDefault: 'S',
          tipEnvio: 'Correo',
          numTelefono: '55443322',
          nomCorreo: 'juan.perez@gmail.com',
          tipLink: 'U',
          diaMes: '0',
          urlLink: 'https://neolink.com.gt/pay/tc-350-perez',
          urlCorto: 'https://lc.bpgt.com.gt/x8y2',
          indEstado: 'A',
          codSku: 'SKU-TC-01',
          nomProducto: 'Pago Tarjeta Juan Pérez',
          codCliente: 'C-99281',
          usuIngreso: 'usuario.demo'
        },
        {
          numCuenta: '2023948576',
          tipCuenta: 'PR',
          monto: 1250.00,
          tipPago: 'Cuotas',
          esDefault: 'N',
          tipEnvio: 'SMS',
          numTelefono: '50129384',
          nomCorreo: 'maria.gomez@yahoo.com',
          tipLink: 'M',
          diaMes: '10',
          urlLink: 'https://neolink.com.gt/pay/pr-1250-gomez',
          urlCorto: 'https://lc.bpgt.com.gt/m4g8',
          indEstado: 'A',
          codSku: 'SKU-PR-02',
          nomProducto: 'Préstamo María Gómez',
          codCliente: 'C-77441',
          usuIngreso: 'usuario.demo'
        },
        {
          numCuenta: '3049586712',
          tipCuenta: 'TC',
          monto: 85.00,
          tipPago: 'Contado',
          esDefault: 'S',
          tipEnvio: 'Ambos',
          numTelefono: '44123456',
          nomCorreo: 'pedro.lopez@outlook.com',
          tipLink: 'U',
          diaMes: '0',
          urlLink: 'https://neolink.com.gt/pay/tc-85-lopez',
          urlCorto: 'https://lc.bpgt.com.gt/p9l4',
          indEstado: 'I',
          codSku: 'SKU-TC-03',
          nomProducto: 'Pago Seguro Pedro López',
          codCliente: 'C-10293',
          usuIngreso: 'usuario.demo'
        },
        {
          numCuenta: '5019283745',
          tipCuenta: 'TC',
          monto: 500.00,
          tipPago: 'Contado',
          esDefault: 'N',
          tipEnvio: 'Correo',
          numTelefono: '55667788',
          nomCorreo: 'ana.martinez@gmail.com',
          tipLink: 'U',
          diaMes: '0',
          urlLink: 'https://neolink.com.gt/pay/tc-500-martinez',
          urlCorto: 'https://lc.bpgt.com.gt/a5m6',
          indEstado: 'C',
          codSku: 'SKU-TC-04',
          nomProducto: 'Pago Tarjeta Ana Martínez',
          codCliente: 'C-38472',
          usuIngreso: 'usuario.demo'
        }
      ]
    });
  }

  if (url.includes('/links/emitir')) {
    return jsonResponse({
      success: true,
      code: 'LINK.EMITTED',
      message: 'Link emitido exitosamente (Simulado)',
      data: 'https://lc.bpgt.com.gt/sim-gen-link-992'
    });
  }

  if (url.includes('/links/validar/')) {
    return jsonResponse({
      success: true,
      code: 'LINK.VALID',
      message: 'Link validado con éxito',
      data: {
        valido: true,
        monto: 350.00,
        nomProducto: 'Pago de Tarjeta Simulado'
      }
    });
  }

  if (url.includes('/links/cancelar')) {
    return jsonResponse({
      success: true,
      code: 'LINK.CANCELLED',
      message: 'El link ha sido cancelado exitosamente en el sistema (Simulado)',
      data: true
    });
  }

  if (url.includes('/links/acortar')) {
    return jsonResponse({
      success: true,
      code: 'LINK.SHORTENED',
      message: 'URL acortada exitosamente',
      data: 'https://lc.bpgt.com.gt/acortado-demo'
    });
  }

  if (url.includes('/links/masivo')) {
    return jsonResponse({
      success: true,
      code: 'LINK.MASSIVE',
      message: 'Carga masiva completada de manera simulada.',
      data: 'Registros procesados: 120, Exitosos: 118, Omitidos: 2'
    });
  }

  if (url.includes('/links/update-estado')) {
    return jsonResponse({
      success: true,
      code: 'LINK.STATE_UPDATED',
      message: 'Estado del link actualizado exitosamente',
      data: true
    });
  }

  if (url.includes('/links/buscar-cta/')) {
    return jsonResponse({
      success: true,
      code: 'CTA.FOUND',
      message: 'Datos de la cuenta de enlace recuperados',
      data: {
        codParametro: 'VISA-ENL-101',
        diaMes: '28',
        proximaFecha: new Date(Date.now() + 7 * 24 * 3600 * 1000).toLocaleDateString()
      }
    });
  }

  if (url.includes('/links/buscar-parametro/')) {
    const cod = url.substring(url.lastIndexOf('/') + 1);
    return jsonResponse({
      success: true,
      code: 'PARAM.FOUND',
      message: 'Parámetro de cuenta recuperado',
      data: {
        codParametro: cod,
        diaMes: '15',
        proximaFecha: '15/06/2026'
      }
    });
  }

  if (url.includes('/links/aplicar-pago')) {
    return jsonResponse({
      success: true,
      message: 'Pago aplicado exitosamente en el sistema central de Promerica.',
      errorMessage: null
    });
  }

  // 4. Endpoints de Clientes
  if (url.includes('/clients/blacklist/')) {
    return jsonResponse({
      success: true,
      code: 'BLACKLIST.OK',
      message: 'El cliente no se encuentra en la lista negra del banco',
      data: false
    });
  }

  if (url.includes('/clients/monto-tc/')) {
    return jsonResponse({
      success: true,
      code: 'AMOUNT.TC',
      message: 'Límite TC obtenido',
      data: 7500.00
    });
  }

  if (url.includes('/clients/monto-pr/')) {
    return jsonResponse({
      success: true,
      code: 'AMOUNT.PR',
      message: 'Monto de cuota PR obtenido',
      data: 1850.50
    });
  }

  if (url.includes('/clients/') && url.includes('/prestamo')) {
    return jsonResponse({
      success: true,
      code: 'LOAN.INFO',
      message: 'Información de préstamo obtenida',
      data: {
        numCuenta: '2023948576',
        moneda: 'GTQ'
      }
    });
  }

  if (url.includes('/clients/') && url.includes('/correo')) {
    return jsonResponse({
      success: true,
      code: 'EMAIL.INFO',
      message: 'Correo obtenido',
      data: 'cliente.demostracion@bancopromerica.com.gt'
    });
  }

  if (url.includes('/clients/') && url.includes('/telefono')) {
    return jsonResponse({
      success: true,
      code: 'PHONE.INFO',
      message: 'Teléfono obtenido',
      data: '5566-7788'
    });
  }

  if (url.includes('/clients/') && url.includes('/cuentas')) {
    return jsonResponse({
      success: true,
      code: 'ACCOUNTS.INFO',
      message: 'Cuentas obtenidas',
      data: ['4019283746', '3049586712', '5019283745', '2023948576']
    });
  }

  if (url.match(/\/clients\/\d+$/) || url.match(/\/clients\/C-\d+$/) || url.match(/\/clients\/\w+$/)) {
    return jsonResponse({
      success: true,
      code: 'CLIENT.INFO',
      message: 'Información de cliente obtenida',
      data: {
        codCliente: 'C-99281',
        nomCliente: 'PÉREZ HERNÁNDEZ, JUAN CARLOS'
      }
    });
  }

  // 5. Endpoints de Transportadoras (Carriers)
  if (url.includes('/carriers/dropdown')) {
    return jsonResponse({
      success: true,
      code: 'CARRIERS.DROPDOWN',
      message: 'Transportadoras para dropdown',
      data: [
        { codTranspo: '100', nomTranspo: 'Guatex Express' },
        { codTranspo: '200', nomTranspo: 'Cargo Express de Guatemala' },
        { codTranspo: '300', nomTranspo: 'Servicios de Entrega Rápida (SER)' }
      ]
    });
  }

  if (url.includes('/carriers/')) {
    return jsonResponse({
      success: true,
      code: 'CARRIER.INFO',
      message: 'Transportadora obtenida',
      data: {
        codTranspo: '100',
        nomTranspo: 'Guatex Express',
        nit: '9928371-8',
        representante: 'Ing. Carlos Archila',
        direccion: 'Calzada Roosevelt 22-43 Zona 11',
        email: 'carlos.archila@guatex.com.gt',
        codAciCli: 'ACI-GTX',
        tipoAcceso: 'API',
        servidor: 'https://api.guatex.com.gt/v1',
        puerto: '443',
        usuario: 'usr_promerica'
      }
    });
  }

  if (url.includes('/carriers') && req.method === 'GET') {
    return jsonResponse({
      success: true,
      code: 'CARRIERS.LIST',
      message: 'Lista de transportadoras obtenida',
      data: [
        {
          codTranspo: '100',
          nomTranspo: 'Guatex Express',
          nit: '9928371-8',
          representante: 'Ing. Carlos Archila',
          direccion: 'Calzada Roosevelt 22-43 Zona 11',
          email: 'carlos.archila@guatex.com.gt',
          codAciCli: 'ACI-GTX',
          tipoAcceso: 'API',
          servidor: 'https://api.guatex.com.gt/v1',
          puerto: '443',
          usuario: 'usr_promerica'
        },
        {
          codTranspo: '200',
          nomTranspo: 'Cargo Express de Guatemala',
          nit: '8837462-1',
          representante: 'Lic. Rodrigo Castellanos',
          direccion: 'Avenida Petapa 34-11 Zona 12',
          email: 'info@cargoexpress.com.gt',
          codAciCli: 'ACI-CXG',
          tipoAcceso: 'SFTP',
          servidor: 'sftp.cargoexpress.com.gt',
          puerto: '22',
          usuario: 'prom_sftp_user'
        }
      ]
    });
  }

  if (url.includes('/carriers') && (req.method === 'POST' || req.method === 'PUT')) {
    return jsonResponse({
      success: true,
      code: 'CARRIER.SAVED',
      message: 'Transportadora guardada correctamente (Simulado)',
      data: req.body
    });
  }

  // 6. Endpoints de Parámetros
  if (url.includes('/parameters') && req.method === 'GET') {
    return jsonResponse({
      success: true,
      code: 'PARAMS.SUCCESS',
      message: 'Parámetros del sistema simulados',
      data: {
        freRevAutorizacion: '15',
        freRevHrsRepetir: '48',
        freGenLink: '10',
        freGenHora: '20:00',
        tcTipTransac: 'V',
        tcSubtipTrans: '02',
        numCtaContaQtz: '3049182736',
        numCtaContaDol: '3049182740',
        codAgencia: '099',
        codTipoTc: '01',
        codSubtipoTc: '01',
        codTipoPr: '05',
        codSubtipoPr: '05',
        codDepartamento: '01',
        codDeptoPr: '01',
        desTransaccion: 'Cobro Neo En Link Promerica',
        apiImagenBase64: 'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==', // 1px transparent png
        msgRemitente: 'Banco Promerica Guatemala',
        msgHeader: 'Estimado Cliente, Banco Promerica le envía su link de pago seguro:',
        msgFooter: 'Si tiene alguna consulta, llámenos al PBX: 1724. Banco Promerica.',
        msgSms: 'Promerica: Adjuntamos link de pago para su cuenta: {link}'
      }
    });
  }

  if (url.includes('/parameters') && req.method === 'PUT') {
    return jsonResponse({
      success: true,
      code: 'PARAMS.UPDATED',
      message: 'Parámetros actualizados exitosamente (Simulado)',
      data: req.body
    });
  }

  return jsonResponse({
    success: false,
    code: 'MOCK.NOT_FOUND',
    message: `Ruta simulada no implementada para la URL: ${url}`
  }, 404);
};
