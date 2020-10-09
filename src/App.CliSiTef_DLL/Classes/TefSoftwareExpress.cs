﻿using App.CliSiTef_DLL.ConstantValues;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace App.CliSiTef_DLL.Classes
{
    public class TefSoftwareExpress
    {
        #region Declaracao das Dll´s Sitef Software Express

        [DllImport("CliSiTef32I.dll")]
        static extern int ConfiguraIntSiTefInterativo(string IPSiTef, string IdLoja, string IdTerminal, string Reservado);

        [DllImport("CliSiTef32I.dll")]
        public static extern int ConfiguraIntSiTefInterativoEx(string IPSiTef, string IdLoja, string IdTerminal, string Reservado, string ParametrosAdicionais);

        [DllImport("CliSiTef32I.dll")]
        static extern int IniciaFuncaoSiTefInterativo(int Funcao, string Valor, string CupomFiscal, string DataFiscal, string HoraFiscal, string Operador, string ParamAdic);

        [DllImport("CliSiTef32I.dll")]
        static extern int ContinuaFuncaoSiTefInterativo(out int Comando, out long TipoCampo, out short TamMinimo, out short TamMaximo, byte[] Buffer, int TamBuffer, int Continua);

        [DllImport("CliSiTef32I.dll")]
        static extern int FinalizaFuncaoSiTefInterativo(short Confirma, string CupomFiscal, string DataFiscal, string HoraFiscal, string ParamAdic);

        [DllImport("CliSiTef32I.dll")]
        static extern int ObtemQuantidadeTransacoesPendentes(string DataFiscal, string CupomFiscal);

        [DllImport("CliSiTef32I.dll")]
        static extern int VerificaPresencaPinPad();

        [DllImport("CliSiTef32I.dll")]
        static extern int KeepAlivePinPad();

        [DllImport("CliSiTef32I.dll")]
        static extern int EscreveMensagemPermanentePinPad(string Mensagem);

        [DllImport("CliSiTef32I.dll")]
        static extern int LeTrilha3(string Mensagem);

        [DllImport("CliSiTef32I.dll")]
        static extern int LeCartaoSeguro(string Mensagem);

        [DllImport("CliSiTef32I.dll")]
        static extern int LeSenhaInterativo(string mensagem);

        [DllImport("CliSiTef32I.dll")]
        static extern int LeSenhaDireto(string ChaveSeguranca, string SenhaCliente);

        [DllImport("CliSiTef32I.dll")]
        static extern int LeSimNaoPinPad(string Mensagem);

        [DllImport("CliSiTef32I.dll")]
        static extern int CorrespondenteBancarioSiTefInterativo(string CupomFiscal, string DataFiscal, string Horario, string Operador, string ParamAdic);

        [DllImport("CliSiTef32I.dll")]
        static extern int ValidaCampoCodigoEmBarras(string Dados, out short Tipo);

        [DllImport("CliSiTef32I.dll")]
        static extern int ObtemVersao(out string VersaoCliSiTef, out string VersaoCliSiTefI);

        [DllImport("CliSiTef32I.dll")]
        static extern int DescarregaMensagens();

        [DllImport("CliSiTef32I.dll")]
        static extern int ObtemInformacoesPinPad(string InfoPinPad);

        #endregion

        public delegate void OnMessageClientHandle(string _mensagem, int _tempoMiliSegundos);
        public event OnMessageClientHandle OnMessageClient;

        public delegate void OnCallFormtHandle(TefFuncaoInterativa _tefFuncaoInterativa);
        public event OnCallFormtHandle OnCallForm;

        TefConfig mTefConfig { get; set; }
        TefTransacao mTefTransacao { get; set; }
        public Cupom gCupomVenda { get; set; }

        public string MensagemTef(int _retornoTef)
        {
            string msg = "Erro não definido";
            switch (_retornoTef)
            {
                case -1:
                    msg = "Módulo não inicializado";
                    break;
                case -2:
                    msg = "Operação cancelada pelo operador";
                    break;
                case -3:
                    msg = "Fornecido um código de função inválido";
                    break;
                case -4:
                    msg = "Falta de memória para rodar a função";
                    break;
                case -5:
                    msg = "Sem comunicação com o SiTef";
                    break;
                case -6:
                    msg = "Operação cancelada pelo usuário";
                    break;
                case -40:
                    msg = "Transação negada pelo SiTef";
                    break;
                case -43:
                    msg = "Falha no pinpad";
                    break;
                case -50:
                    msg = "Transação não segura";
                    break;
                case -100:
                    msg = "Erro interno do módulo";
                    break;
                case 0:
                    msg = "";
                    break;
                case 1:
                    msg = "Endereço IP inválido ou não resolvido";
                    break;
                case 2:
                    msg = "Código da loja inválido";
                    break;
                case 3:
                    msg = "Código de terminal inválido";
                    break;
                case 6:
                    msg = "Erro na inicialização do Tcp/Ip";
                    break;
                case 7:
                    msg = "Falta de memória";
                    break;
                case 8:
                    msg = "Não encontrou a CliSiTef ou ela está com problemas";
                    break;
                case 9:
                    msg = "Configuração de servidores SiTef foi excedida";
                    break;
                case 10:
                    msg = "Erro de acesso na pasta CliSiTef(possível falta de permissão para escrita)";
                    break;
                case 11:
                    msg = "Dados inválidos passados pela automaçãoo";
                    break;
                case 12:
                    msg = "Modo seguro não ativo(possível falta de configuração no servidor SiTef do arquivo.cha)";
                    break;
                case 13:
                    msg = "Caminho DLL inválido(o caminho completo das bibliotecas está muito grande)";
                    break;
                case 50001:
                    msg = "PinPad não encontrado";
                    break;
                default:
                    break;
            }
            return msg;
        }
        void TefRetornoAdicionar(TefRetorno _obj, TefTransacao _tefTransacao, bool _substituirValor = true)
        {
            TefRetorno obj = _tefTransacao.Retornos.Find(p => p.Codigo == _obj.Codigo && p.Indice == _obj.Indice);
            if (obj != null)
            {
                if (_substituirValor)
                {
                    obj.Codigo = _obj.Codigo;
                    obj.Indice = _obj.Indice;
                    obj.Valor = _obj.Valor;
                }
            }
            else
                _tefTransacao.Retornos.Add(_obj);
        }
        void GerarArquivoRetornoDaTransacao()
        {
            if (gCupomVenda != null)
            {
                if (gCupomVenda.Transacoes.Count > 0)
                {
                    int transacaoId = 1;
                    foreach (TefTransacao itemTefTransacao in gCupomVenda.Transacoes)
                    {
                        if (itemTefTransacao.Retornos.Count > 0)
                        {
                            string path = mTefConfig.Tef_PathArquivos + "\\TefRetorno";
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);

                            if (Directory.Exists(path))
                            {
                                var lst = itemTefTransacao.Retornos.OrderBy(p => p.Codigo).ThenBy(p => p.Indice).ToList();
                                using (StreamWriter sr = File.AppendText(path + "\\" + "Tef" + gCupomVenda.TipoOperacao + "_" + gCupomVenda.DocumentoVinculado + "_T" + transacaoId.ToString("000") + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".tef"))
                                {
                                    foreach (var item in lst)
                                    {
                                        sr.WriteLine(item.Codigo.ToString("000") + "-" + item.Indice.ToString("000") + "=" + item.Valor);
                                        sr.Flush();
                                    }
                                }
                                transacaoId++;
                            }
                        }
                    }
                }
            }
        }
        string RemoverQuebraDeLinhas(string _texto)
        {
            if (string.IsNullOrWhiteSpace(_texto))
                return "";
            _texto = Regex.Replace(_texto, @"\r\n?|\n|\t", " "); //Remover Quebra de Linha (ENTER)
            _texto = Regex.Replace(_texto, @"\s{2,}", " "); //Remover espaços a mais no meio da palavra

            return _texto;
        }

        public int InicializarTef(TefConfig _tefConfig)
        {
            mTefConfig = _tefConfig;
            int sts = ConfiguraIntSiTefInterativoEx(_tefConfig.Tef_Ip, _tefConfig.Tef_Empresa, "IP" + _tefConfig.Tef_Terminal, "0", "[VersaoAutomacaoCielo=G310];[ParmsClient=1=" + _tefConfig.Tef_EmpresaCnpj + ";2=" + _tefConfig.Tef_SoftwareHouseCnpj + "]");
            if (sts == 0)
            {
                if (_tefConfig.Tef_PinPadVerificar)
                {
                    int stsPinPad = VerificaPresencaPinPad();
                    if (stsPinPad > 0)
                        EscreveMensagemPermanentePinPad(_tefConfig.Tef_PinPadMensagem);
                    else
                        sts = 50001;
                }
            }
            return sts;
        }
        int ContinuarRequisicao()
        {
            byte[] valorBuffer = new byte[20000];
            int result;
            int continua = 0;
            int taxas = 1;
            string captionMenu = "";
            string captionCarteiraDigital = "";
            bool interromper = false;

            do
            {
                result = ContinuaFuncaoSiTefInterativo(out int proximoComando, out long tipoCampo, out short tamanhoMinimo, out short tamanhoMaximo, valorBuffer, valorBuffer.Length, continua);

                continua = 0;
                string mensagem = Encoding.UTF8.GetString(valorBuffer).Replace("\0", "").Trim();
                string respostaSitef = "";
                bool voltar = false;
                bool digitado = true;

                if (result == 10000)
                {
                    switch (proximoComando)
                    {
                        case 0: //Está devolvendo um valor para, se desejado, ser armazenado pela automação
                            #region Trata Tipo de Campo

                            if (tipoCampo == 0)
                            {
                                TefRetorno obj1 = new TefRetorno(1, 0, mensagem);
                                TefRetornoAdicionar(obj1, mTefTransacao);
                            }
                            else if (tipoCampo == 100)
                            {
                                TefRetorno obj11 = new TefRetorno(11, 0, mensagem);
                                TefRetornoAdicionar(obj11, mTefTransacao);

                                string msgAut = mensagem.PadRight(4, '0');
                                TefRetorno obj731 = new TefRetorno(731, 0, msgAut.Substring(0, 2));
                                TefRetornoAdicionar(obj731, mTefTransacao);
                                ModalidadePagamentoGrupoConst grupo = ModalidadePagamentoGrupo.RetornarModalidadePagamentoGrupo(msgAut.Substring(0, 2));
                                if (grupo != null)
                                {
                                    TefRetorno obj731_1 = new TefRetorno(731, 1, grupo.Nome);
                                    TefRetornoAdicionar(obj731_1, mTefTransacao);
                                }

                                TefRetorno obj732 = new TefRetorno(732, 0, msgAut.Substring(2, 2));
                                TefRetornoAdicionar(obj732, mTefTransacao);
                                ModalidadePagamentoSubGrupoConst subgrupo = ModalidadePagamentoSubGrupo.RetornarModalidadePagamentoSubGrupo(msgAut.Substring(2, 2));
                                if (subgrupo != null)
                                {
                                    TefRetorno obj732_1 = new TefRetorno(732, 1, subgrupo.Nome);
                                    TefRetornoAdicionar(obj732_1, mTefTransacao);
                                }
                            }
                            else if (tipoCampo == 105)
                            {
                                string msgData = mensagem.Substring(6, 2) + mensagem.Substring(4, 2) + mensagem.Substring(0, 4);
                                TefRetorno obj22 = new TefRetorno(22, 0, msgData);
                                TefRetornoAdicionar(obj22, mTefTransacao);

                                string msgHora = mensagem.Substring(8);
                                TefRetorno obj23 = new TefRetorno(23, 0, msgHora);
                                TefRetornoAdicionar(obj23, mTefTransacao);
                            }
                            else if (tipoCampo == 106)
                            {
                                if (!string.IsNullOrWhiteSpace(mensagem))
                                {
                                    TefRetorno obj748_1 = new TefRetorno(748, 1, mensagem);
                                    TefRetornoAdicionar(obj748_1, mTefTransacao);

                                    BandeiraPadraoConst obj = BandeiraPadrao.RetornarBandeiraPadrao(Convert.ToInt32(mensagem));
                                    if (obj != null)
                                    {
                                        TefRetorno obj748_2 = new TefRetorno(748, 2, obj.NomeTipoCodigo);
                                        TefRetornoAdicionar(obj748_2, mTefTransacao);
                                    }
                                }
                            }
                            else if (tipoCampo == 107)
                            {
                                if (!string.IsNullOrWhiteSpace(mensagem))
                                {
                                    captionCarteiraDigital = mensagem;
                                    TefRetorno obj748 = new TefRetorno(748, 0, mensagem);
                                    TefRetornoAdicionar(obj748, mTefTransacao);
                                }
                            }
                            else if (tipoCampo == 111)
                            {
                                OnMessageClient?.Invoke(mensagem, 500);
                            }
                            else if (tipoCampo == 121)
                            {
                                string[] viaCliente = mensagem.Split('\n', '\r');
                                TefRetorno obj712 = new TefRetorno(712, 0, viaCliente.Length.ToString());
                                TefRetornoAdicionar(obj712, mTefTransacao);

                                for (int i = 0; i < viaCliente.Length; i++)
                                {
                                    TefRetorno obj713 = new TefRetorno(713, i, "\"" + viaCliente[i] + "\"");
                                    TefRetornoAdicionar(obj713, mTefTransacao);
                                }
                            }
                            else if (tipoCampo == 122)
                            {
                                string[] viaEstab = mensagem.Split('\n', '\r');
                                TefRetorno obj714 = new TefRetorno(714, 0, viaEstab.Length.ToString());
                                TefRetornoAdicionar(obj714, mTefTransacao);

                                for (int i = 0; i < viaEstab.Length; i++)
                                {
                                    TefRetorno obj715 = new TefRetorno(715, i, "\"" + viaEstab[i] + "\"");
                                    TefRetornoAdicionar(obj715, mTefTransacao);
                                }
                            }
                            else if (tipoCampo == 123)
                            {
                                if (!string.IsNullOrWhiteSpace(mensagem))
                                {
                                    ComprovanteTipoConst obj = ComprovanteTipo.RetornarComprovanteTipo(mensagem);
                                    if (obj != null)
                                    {
                                        TefRetorno obj712_1 = new TefRetorno(712, 1, obj.CodigoNome);
                                        TefRetornoAdicionar(obj712_1, mTefTransacao);

                                        TefRetorno obj714_1 = new TefRetorno(714, 1, obj.CodigoNome);
                                        TefRetornoAdicionar(obj714_1, mTefTransacao);
                                    }
                                }
                            }
                            else if (tipoCampo == 131)
                            {
                                TefRetorno obj10 = new TefRetorno(10, 0, mensagem);
                                TefRetornoAdicionar(obj10, mTefTransacao);

                                var obj = RedeAutorizadora.RetornarAutorizadora(obj10.Valor);
                                if (obj != null)
                                {
                                    TefRetorno obj10_1 = new TefRetorno(10, 1, obj.Nome);
                                    TefRetornoAdicionar(obj10_1, mTefTransacao);
                                }
                            }
                            else if (tipoCampo == 132)
                            {
                                TefRetorno obj748_1 = new TefRetorno(748, 1, mensagem);
                                TefRetornoAdicionar(obj748_1, mTefTransacao);

                                BandeiraPadraoConst obj = BandeiraPadrao.RetornarBandeiraPadrao(Convert.ToInt32(mensagem));
                                if (obj != null)
                                {
                                    TefRetorno obj748_2 = new TefRetorno(748, 2, obj.NomeTipoCodigo);
                                    TefRetornoAdicionar(obj748_2, mTefTransacao);
                                }
                            }
                            else if (tipoCampo == 133)
                            {
                                TefRetorno obj13 = new TefRetorno(13, 0, mensagem);
                                TefRetornoAdicionar(obj13, mTefTransacao);
                            }
                            else if (tipoCampo == 134)
                            {
                                TefRetorno obj12 = new TefRetorno(12, 0, mensagem);
                                TefRetornoAdicionar(obj12, mTefTransacao);
                            }
                            else if (tipoCampo == 156)
                            {
                                TefRetorno obj748 = new TefRetorno(748, 0, mensagem);
                                TefRetornoAdicionar(obj748, mTefTransacao);
                            }
                            else if (tipoCampo == 158)
                            {
                                TefRetorno obj739 = new TefRetorno(739, 0, mensagem);
                                TefRetornoAdicionar(obj739, mTefTransacao);
                            }
                            else if (tipoCampo == 590)
                            {
                                TefRetorno obj742 = new TefRetorno(742, 0, mensagem);
                                TefRetornoAdicionar(obj742, mTefTransacao);
                            }
                            else if (tipoCampo == 591)
                            {
                                if (!string.IsNullOrWhiteSpace(mensagem))
                                {
                                    decimal valorRecarga = Convert.ToDecimal(mensagem) / 100M;
                                    TefRetorno obj742_1 = new TefRetorno(742, 1, valorRecarga.ToString("N2"));
                                    TefRetornoAdicionar(obj742_1, mTefTransacao);
                                }
                            }
                            else if (tipoCampo == 800)
                            {
                                TefRetorno obj27 = new TefRetorno(27, 0, mensagem);
                                TefRetornoAdicionar(obj27, mTefTransacao);
                            }
                            else if (tipoCampo == 2021)
                            {
                                TefRetorno obj740 = new TefRetorno(740, 0, mensagem);
                                TefRetornoAdicionar(obj740, mTefTransacao);
                            }
                            else if (tipoCampo == 2022)
                            {
                                string msgAut = mensagem.PadRight(4, '0');
                                TefRetorno obj747 = new TefRetorno(747, 0, msgAut.Substring(2, 2) + msgAut.Substring(0, 2));
                                TefRetornoAdicionar(obj747, mTefTransacao);
                            }
                            else if (tipoCampo == 2023)
                            {
                                TefRetorno obj741 = new TefRetorno(741, 0, mensagem);
                                TefRetornoAdicionar(obj741, mTefTransacao);
                            }

                            #endregion
                            break;
                        case 1: //Mensagem para o visor do operador
                            OnMessageClient?.Invoke(mensagem, 250);
                            break;
                        case 2: //Mensagem para o visor do cliente
                            OnMessageClient?.Invoke(mensagem, 250);
                            break;
                        case 3: //Mensagem para os dois visores
                            OnMessageClient?.Invoke(mensagem, 250);
                            break;
                        case 4: //Texto que deverá ser utilizado como cabeçalho na apresentação do menu (Comando 21)
                            captionMenu = mensagem;
                            break;
                        case 11: //Deve remover a mensagem apresentada no visor do operador
                            mensagem = "";
                            OnMessageClient?.Invoke(mensagem, 0);
                            break;
                        case 12: //Deve remover a mensagem apresentada no visor do cliente
                            mensagem = "";
                            OnMessageClient?.Invoke(mensagem, 0);
                            break;
                        case 13: //Deve remover mensagem apresentada no visor do operador e do cliente
                            mensagem = "";
                            OnMessageClient?.Invoke(mensagem, 0);
                            break;
                        case 14: //Deve limpar o texto utilizado como cabeçalho na apresentação do menu
                            captionMenu = "";
                            break;
                        case 15: //Cabeçalho a ser apresentado pela aplicação
                            break;
                        case 16: //Deve remover o cabeçalho
                            captionMenu = "";
                            break;
                        case 20: //Deve obter uma resposta do tipo SIM/NÃO.
                            if (string.IsNullOrWhiteSpace(mensagem))
                                mensagem = "Confirma?";
                            TefFuncaoInterativa objForm20 = new TefFuncaoInterativa
                            {
                                DataType = DataTypeEnum.Confirmation,
                                TipoCampo = tipoCampo,
                                RespostaSitef = "1",
                                Mensagem = mensagem
                            };
                            OnCallForm?.Invoke(objForm20);
                            respostaSitef = objForm20.RespostaSitef;
                            interromper = objForm20.Interromper;
                            break;
                        case 21: //Deve apresentar um menu de opções e permitir que o usuário selecione uma delas. Na chamada o parâmetro Buffer contém as opções no formato 1:texto;2:texto;...i:Texto;... A rotina da aplicação deve apresentar as opções da forma que ela desejar (não sendo necessário incluir os índices 1,2, ...) e após a seleção feita pelo usuário, retornar em Buffer o índice i escolhido pelo operador (em ASCII)
                            TefFuncaoInterativa objForm21 = new TefFuncaoInterativa
                            {
                                DataType = DataTypeEnum.Menu,
                                Titulo = captionMenu,
                                ItensMenu = mensagem.Split(';')
                            };
                            OnCallForm?.Invoke(objForm21);
                            respostaSitef = objForm21.RespostaSitef;
                            interromper = objForm21.Interromper;
                            break;
                        case 22: //Deve aguardar uma tecla do operador. É utilizada quando se deseja que o operador seja avisado de alguma mensagem apresentada na tela
                            if (string.IsNullOrWhiteSpace(mensagem))
                                mensagem = "Aguarde .....";
                            TefFuncaoInterativa objForm22 = new TefFuncaoInterativa
                            {
                                DataType = DataTypeEnum.Await,
                                Mensagem = mensagem
                            };
                            OnCallForm?.Invoke(objForm22);
                            respostaSitef = "";
                            break;
                        case 23: //Este comando indica que a rotina está perguntando para a aplicação se ele deseja interromper o processo de coleta de dados ou não. Esse código ocorre quando a CliSiTef está acessando algum periférico e permite que a automação interrompa esse acesso (por exemplo: aguardando a passagem de um cartão pela leitora ou a digitação de senha pelo cliente)
                            break;
                        case 29: //Deve ser fornecido um campo, sem captura, cujo tamanho está entre TamMinimo e TamMaximo. O campo deve ser devolvido em Buffer
                            break;
                        case 30: //Deve ser lido um campo cujo tamanho está entre TamMinimo e TamMaximo. O campo lido deve ser devolvido em Buffer
                            TefFuncaoInterativa objForm30 = new TefFuncaoInterativa
                            {
                                DataType = DataTypeEnum.Numeric,
                                TipoCampo = tipoCampo,
                                TamanhoMinimo = tamanhoMinimo,
                                TamanhoMaximo = tamanhoMaximo,
                                Titulo = mensagem
                            };
                            OnCallForm?.Invoke(objForm30);
                            respostaSitef = objForm30.RespostaSitef;
                            interromper = objForm30.Interromper;
                            if (!interromper)
                            {
                                if (tipoCampo == 505)
                                {
                                    TefRetorno obj505 = new TefRetorno(505, 0, respostaSitef);
                                    TefRetornoAdicionar(obj505, mTefTransacao);
                                }
                            }
                            break;
                        case 31: //Deve ser lido o número de um cheque. A coleta pode ser feita via leitura de CMC-7 ou pela digitação da primeira linha do cheque. No retorno deve ser devolvido em Buffer “0:” ou “1:” seguido do número coletado manualmente ou pela leitura do CMC-7, respectivamente. Quando o número for coletado manualmente o formato é o seguinte: Compensação (3), Banco (3), Agencia (4), C1 (1), ContaCorrente (10), C2 (1), Numero do Cheque (6) e C3 (1), nesta ordem. Notar que estes campos são os que estão na parte superior de um cheque e na ordem apresentada. Sugerimos que na coleta seja apresentada uma interface que permita ao operador identificar e digitar adequadamente estas informações de forma que a consulta não seja feita com dados errados, retornando como bom um cheque com problemas
                            break;
                        case 34: //Deve ser lido um campo monetário ou seja, aceita o delimitador de centavos e devolvido no parâmetro Buffer
                            TefFuncaoInterativa objForm34 = new TefFuncaoInterativa
                            {
                                DataType = DataTypeEnum.Currency,
                                TipoCampo = tipoCampo,
                                TamanhoMinimo = tamanhoMinimo,
                                TamanhoMaximo = tamanhoMaximo,
                                Titulo = mensagem
                            };
                            OnCallForm?.Invoke(objForm34);
                            respostaSitef = objForm34.RespostaSitef;
                            interromper = objForm34.Interromper;
                            if (tipoCampo == 504 || tipoCampo == 130)
                            {
                                if (!string.IsNullOrWhiteSpace(respostaSitef) && Convert.ToDecimal(respostaSitef) > 0)
                                {
                                    string valor = Convert.ToDecimal(respostaSitef).ToString("N2");
                                    TefRetorno obj3 = new TefRetorno(3, taxas, valor + "|" + RemoverQuebraDeLinhas(mensagem));
                                    TefRetornoAdicionar(obj3, mTefTransacao);
                                    taxas++;
                                }
                            }
                            break;
                        case 35: //Deve ser lido um código em barras ou o mesmo deve ser coletado manualmente. No retorno Buffer deve conter “0:” ou “1:” seguido do código em barras coletado manualmente ou pela leitora, respectivamente. Cabe ao aplicativo decidir se a coleta será manual ou através de uma leitora. Caso seja coleta manual, recomenda-se seguir o procedimento descrito na rotina ValidaCampoCodigoEmBarras de forma a tratar um código em barras da forma mais genérica possível, deixando o aplicativo de automação independente de futuras alterações que possam surgir nos formatos em barras. No retorno do Buffer também pode ser passado “2:”, indicando que a coleta foi cancelada, porém o fluxo não será interrompido, logo no caso de pagamentos múltiplos, todos os documentados coletados anteriormente serão mantidos e o fluxo retomado, permitindo a efetivação de tais pagamentos.
                            break;
                        case 41: //Análogo ao Comando 30 (TextInputNeeded), porém o campo deve ser coletado de forma mascarada (senha).
                            break;
                        case 42: //Deve apresentar um menu de opções e permitir que o usuário selecione uma delas.
                            break;
                        case 50:
                            TefFuncaoInterativa objForm50 = new TefFuncaoInterativa
                            {
                                DataType = DataTypeEnum.QrCode,
                                TipoCampo = tipoCampo,
                                Titulo = captionCarteiraDigital,
                                Mensagem = mensagem
                            };
                            OnCallForm?.Invoke(objForm50);
                            respostaSitef = objForm50.RespostaSitef;
                            interromper = objForm50.Interromper;
                            captionCarteiraDigital = "";
                            break;
                        case 51:
                            OnMessageClient?.Invoke(mensagem, 250);
                            break;
                        case 52:
                            OnMessageClient?.Invoke(mensagem, 250);
                            break;
                        case 99:
                            break;
                        default:
                            break;
                    }
                }
                if (voltar)
                    continua = 1;
                else if (!digitado || interromper)
                    continua = -1;

                valorBuffer = Encoding.ASCII.GetBytes(respostaSitef + new string('\0', 20000 - respostaSitef.Length));
            } while (result == 10000);
            return result;
        }
        int FazerRequisicao(int _funcao, string _header, decimal _valor = 0M, string _documento = "", string _parametrosAdicionais = "", string _operador = "")
        {
            if (string.IsNullOrWhiteSpace(_documento))
                _documento = DateTime.Now.ToString("HHmmss");

            if (_parametrosAdicionais.IndexOf(@"{TipoTratamento=4}", StringComparison.Ordinal) == -1 && (_header.Contains("ADM") || _header.Contains("CRT") || _header.Contains("CHQ")))
                _parametrosAdicionais += "{TipoTratamento=4}";
            if (_header.Contains("CRT"))
                _parametrosAdicionais += "{DevolveStringQRCode=1}";

            var dataHora = DateTime.Now;
            var dataStr = dataHora.ToString("yyyyMMdd");
            var horaStr = dataHora.ToString("HHmmss");
            var valorStr = _valor.ToString("N2");

            return IniciaFuncaoSiTefInterativo(_funcao, valorStr, _documento, dataStr, horaStr, _operador, _parametrosAdicionais);
        }
        void FinalizarOperacao(short _confirma, string _documentoVinculado = "")
        {
            string dataStr = DateTime.Now.ToString("yyyyMMdd");
            string horaStr = DateTime.Now.ToString("HHmmss");
            int doc = new Random().Next(999999);
            if (string.IsNullOrWhiteSpace(_documentoVinculado))
                _documentoVinculado = doc.ToString("000000");
            FinalizaFuncaoSiTefInterativo(_confirma, _documentoVinculado, dataStr, horaStr, null);
        }

        public int Atv()
        {
            int sts = FazerRequisicao(111, "ATV");
            if (sts == 10000)
                sts = ContinuarRequisicao();
            return sts;
        }
        public int Adm(string _documentoVinculado = "")
        {
            int sts = FazerRequisicao(110, "ADM", _documento: _documentoVinculado);
            if (sts == 10000)
            {
                #region Retornos TEF

                mTefTransacao = new TefTransacao
                {
                    DocumentoVinculado = _documentoVinculado,
                    ValorTransacao = 0M
                };
                gCupomVenda.Transacoes.Add(mTefTransacao);

                TefRetorno obj0 = new TefRetorno(0, 0, "ADM");
                TefRetornoAdicionar(obj0, mTefTransacao);

                TefRetorno obj2 = new TefRetorno(2, 0, _documentoVinculado);
                TefRetornoAdicionar(obj2, mTefTransacao);

                TefRetorno obj2_1 = new TefRetorno(2, 1, mTefTransacao.IdentificadorTransacao.ToString());
                TefRetornoAdicionar(obj2_1, mTefTransacao);

                TefRetorno obj4 = new TefRetorno(4, 0, "0");
                TefRetornoAdicionar(obj4, mTefTransacao);

                TefRetorno obj718 = new TefRetorno(718, 0, "IP" + mTefConfig.Tef_Terminal);
                TefRetornoAdicionar(obj718, mTefTransacao);

                TefRetorno obj719 = new TefRetorno(719, 0, mTefConfig.Tef_Empresa);
                TefRetornoAdicionar(obj719, mTefTransacao);

                #endregion

                sts = ContinuarRequisicao();
            }
            if (sts == 0)
                Cnf(_documentoVinculado: _documentoVinculado);
            return sts;
        }
        public int Crt(decimal _valor, string _documentoVinculado = "", string _operador = "", int _funcao = 0, bool _confirmarCnf = true)
        {
            string parametrosAdicionais = ""; // "[10]"; //Cheques
            int sts = FazerRequisicao(_funcao, "CRT", _valor, _documentoVinculado, parametrosAdicionais, _operador);
            if (sts == 10000)
            {
                #region Retornos TEF

                mTefTransacao = new TefTransacao
                {
                    DocumentoVinculado = _documentoVinculado,
                    ValorTransacao = _valor
                };
                gCupomVenda.Transacoes.Add(mTefTransacao);

                TefRetorno obj0 = new TefRetorno(0, 0, "CRT");
                TefRetornoAdicionar(obj0, mTefTransacao);

                TefRetorno obj2 = new TefRetorno(2, 0, _documentoVinculado);
                TefRetornoAdicionar(obj2, mTefTransacao);

                TefRetorno obj2_1 = new TefRetorno(2, 1, mTefTransacao.IdentificadorTransacao.ToString());
                TefRetornoAdicionar(obj2_1, mTefTransacao);

                TefRetorno obj3 = new TefRetorno(3, 0, _valor.ToString("N2"));
                TefRetornoAdicionar(obj3, mTefTransacao);

                TefRetorno obj4 = new TefRetorno(4, 0, "0");
                TefRetornoAdicionar(obj4, mTefTransacao);

                TefRetorno obj718 = new TefRetorno(718, 0, "IP" + mTefConfig.Tef_Terminal);
                TefRetornoAdicionar(obj718, mTefTransacao);

                TefRetorno obj719 = new TefRetorno(719, 0, mTefConfig.Tef_Empresa);
                TefRetornoAdicionar(obj719, mTefTransacao);

                #endregion

                sts = ContinuarRequisicao();
            }
            if (sts == 0)
            {
                #region Retornos TEF

                TefRetorno obj9 = new TefRetorno(9, 0, "0");
                TefRetornoAdicionar(obj9, mTefTransacao);

                #endregion

                if (_confirmarCnf)
                    Cnf(_documentoVinculado: _documentoVinculado);
            }
            return sts;
        }
        public int Cnc(string _documentoVinculado, string _operador = "", int _funcao = 200)
        {
            string parametrosAdicionais = ""; // "[10]"; //Cheques
            int sts = FazerRequisicao(_funcao, "CNC", 0M, _documentoVinculado, parametrosAdicionais, _operador);
            if (sts == 10000)
            {
                #region Retornos TEF

                mTefTransacao = new TefTransacao
                {
                    DocumentoVinculado = _documentoVinculado,
                    ValorTransacao = 0M
                };
                gCupomVenda.Transacoes.Add(mTefTransacao);

                TefRetorno obj0 = new TefRetorno(0, 0, "CNC");
                TefRetornoAdicionar(obj0, mTefTransacao);

                TefRetorno obj2 = new TefRetorno(2, 0, _documentoVinculado);
                TefRetornoAdicionar(obj2, mTefTransacao);

                TefRetorno obj2_1 = new TefRetorno(2, 1, mTefTransacao.IdentificadorTransacao.ToString());
                TefRetornoAdicionar(obj2_1, mTefTransacao);

                TefRetorno obj4 = new TefRetorno(4, 0, "0");
                TefRetornoAdicionar(obj4, mTefTransacao);

                TefRetorno obj718 = new TefRetorno(718, 0, "IP" + mTefConfig.Tef_Terminal);
                TefRetornoAdicionar(obj718, mTefTransacao);

                TefRetorno obj719 = new TefRetorno(719, 0, mTefConfig.Tef_Empresa);
                TefRetornoAdicionar(obj719, mTefTransacao);

                #endregion

                sts = ContinuarRequisicao();
            }
            if (sts == 0)
                Cnf(_documentoVinculado: _documentoVinculado);
            return sts;
        }
        public int RecargaCelular(string _documentoVinculado = "")
        {
            int sts = FazerRequisicao(300, "CEL", _documento: _documentoVinculado);
            if (sts == 10000)
            {
                #region Retornos TEF

                mTefTransacao = new TefTransacao
                {
                    DocumentoVinculado = _documentoVinculado,
                    ValorTransacao = 0M
                };
                gCupomVenda.Transacoes.Add(mTefTransacao);

                TefRetorno obj0 = new TefRetorno(0, 0, "CEL");
                TefRetornoAdicionar(obj0, mTefTransacao);

                TefRetorno obj2 = new TefRetorno(2, 0, _documentoVinculado);
                TefRetornoAdicionar(obj2, mTefTransacao);

                TefRetorno obj2_1 = new TefRetorno(2, 1, mTefTransacao.IdentificadorTransacao.ToString());
                TefRetornoAdicionar(obj2_1, mTefTransacao);

                TefRetorno obj4 = new TefRetorno(4, 0, "0");
                TefRetornoAdicionar(obj4, mTefTransacao);

                TefRetorno obj718 = new TefRetorno(718, 0, "IP" + mTefConfig.Tef_Terminal);
                TefRetornoAdicionar(obj718, mTefTransacao);

                TefRetorno obj719 = new TefRetorno(719, 0, mTefConfig.Tef_Empresa);
                TefRetornoAdicionar(obj719, mTefTransacao);

                #endregion

                sts = ContinuarRequisicao();
            }
            if (sts == 0)
                Cnf(_documentoVinculado: _documentoVinculado);
            return sts;
        }
        public void Cnf(bool _gerarArquivo = true, string _documentoVinculado = "")
        {
            FinalizarOperacao(1, _documentoVinculado);

            #region Retornos TEF

            TefRetorno obj729 = new TefRetorno(729, 0, "1");
            TefRetornoAdicionar(obj729, mTefTransacao);

            TefRetorno obj999 = new TefRetorno(999, 0, "0");
            TefRetornoAdicionar(obj999, mTefTransacao);

            if (_gerarArquivo)
                GerarArquivoRetornoDaTransacao();

            #endregion
        }
    }
}