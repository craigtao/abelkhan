﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace logic
{
	public class gatemanager
	{
		public gatemanager(service.connectnetworkservice _conn)
		{
			_gate_conn = _conn;
			current_client_uuid = "";

			clients = new Dictionary<string, gateproxy>();
			ch_gateproxys = new Dictionary<juggle.Ichannel, gateproxy>();
			gates = new Dictionary<string, gateproxy>();
		}

		public void connect_gate(String uuid, String ip, short port)
		{
			var ch = _gate_conn.connect(ip, port);
			gates.Add(uuid, new gateproxy(ch));
			ch_gateproxys.Add(ch, gates[uuid]);
			gates[uuid].reg_logic();
		}

		public gateproxy get_gateproxy(juggle.Ichannel gate_ch)
		{
			if (ch_gateproxys.ContainsKey(gate_ch))
			{
				return ch_gateproxys[gate_ch];
			}

			return null;
		}

		public delegate void clientConnecthandle(String client_uuid);
		public event clientConnecthandle clientConnect;
		public void client_connect(String client_uuid, juggle.Ichannel gate_ch)
		{
			if (ch_gateproxys.ContainsKey(gate_ch))
			{
				var _proxy = ch_gateproxys[gate_ch];
				clients.Add(client_uuid, _proxy);

				if (clientConnect != null)
				{
					clientConnect(client_uuid);
				}
			}
		}

		public delegate void clientDisconnecthandle(String client_uuid);
		public event clientDisconnecthandle clientDisconnect;
		public void client_disconnect(String client_uuid)
		{
            if (clients.ContainsKey(client_uuid))
            {
                clients.Remove(client_uuid);

                if (clientDisconnect != null)
                {
                    clientDisconnect(client_uuid);
                }
            }
		}

        public delegate void clientExceptionhandle(String client_uuid);
        public event clientExceptionhandle clientException;
        public void client_exception(String client_uuid)
        {
            if (clientException != null)
            {
                clientException(client_uuid);
            }
        }

        public void call_client(String uuid, String module, String func, params object[] _argvs)
		{
			if (clients.ContainsKey(uuid))
			{
				ArrayList _argvs_list = new ArrayList();
				foreach (var o in _argvs)
				{
					_argvs_list.Add(o);
				}

				clients[uuid].forward_logic_call_client(uuid, module, func, _argvs_list);
			}
		}

		public void call_group_client(ArrayList uuids, String module, String func, params object[] _argvs)
		{
			foreach (var _proxy in ch_gateproxys)
			{
				ArrayList _argvs_list = new ArrayList();
				foreach (var o in _argvs)
				{
					_argvs_list.Add(o);
				}

				_proxy.Value.forward_logic_call_group_client(uuids, module, func, _argvs_list);
			}
		}

		public void call_global_client(String module, String func, params object[] _argvs)
		{
			foreach (var _proxy in ch_gateproxys)
			{
				ArrayList _argvs_list = new ArrayList();
				foreach (var o in _argvs)
				{
					_argvs_list.Add(o);
				}

				_proxy.Value.forward_logic_call_global_client(module, func, _argvs_list);
			}
		}
	
		public String current_client_uuid;

		private Dictionary<String, gateproxy> clients;

		private Dictionary<juggle.Ichannel, gateproxy> ch_gateproxys;
		private Dictionary<String, gateproxy> gates;
		private service.connectnetworkservice _gate_conn;
	}
}

