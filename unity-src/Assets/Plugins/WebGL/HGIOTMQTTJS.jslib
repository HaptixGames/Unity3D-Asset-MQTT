var HGIOTMQTTJS = {
	//$IOTMQTT__deps: ['$?'],
	//$IOTMQTT__postset: '',
	$IOTMQTT: {
		log: {
			source: "HGIOTMQTTJS",
			enabled: true
		},
		instances : [],
		hookClient: function(instanceId) {
			var instance = IOTMQTT.instances[instanceId];

			instance.client.onConnectionLost = function(response) {
				response.__instanceId = instanceId;
				response.autoReconnect = instance.autoReconnect;
				CLOGGER(IOTMQTT.log,'onConnectionLost: ' + JSON.stringify(response));
				SendMessage(instance.gameObjectName,'HGIOTMQTTJS_onConnectionLost', JSON.stringify(response));
				if(instance.autoReconnect && response.errorCode > 0) { IOTMQTT.reconnect(instanceId); }
			};

			instance.client.onMessageArrived = function(message) {
				var clone = { 
					topic: message.destinationName,
					payloadString: message.payloadString,
					isDuplicate: message.duplicate,
					isRetained: message.retained,
					qualityOfService: message.qos,
					__instanceId: instanceId
				};
				CLOGGER(IOTMQTT.log,'onMessageArrived: ' + JSON.stringify(clone));
				SendMessage(instance.gameObjectName,'HGIOTMQTTJS_onMessageArrived', JSON.stringify(clone));
			};

			instance.client.onMessageDelivered = function(message) {
				var clone = { 
					topic: message.destinationName,
					payloadString: message.payloadString,
					isDuplicate: message.duplicate,
					isRetained: message.retained,
					qualityOfService: message.qos,
					__instanceId: instanceId
				};
				CLOGGER(IOTMQTT.log,'onMessageDelivered: ' + JSON.stringify(clone));
				SendMessage(instance.gameObjectName,'HGIOTMQTTJS_onMessageDelivered', JSON.stringify(clone));
			};
		},
		recreateClient: function(instanceId) {
			var instance = IOTMQTT.instances[instanceId];
			var newClientId = 'UNITY3D-' + Math.floor((Math.random() * 1000000) + 1);

			instance.client = new Paho.MQTT.Client(
				instance.client.host, 
				instance.client.port, 
				instance.client.path, 
				newClientId);

			IOTMQTT.hookClient(instanceId);
		},
		createClient: function(gameObjectName,
		 host,port,clientId,path,
		 keepAliveInterval,cleanSession,invocationContext,username,password,useSSL,
		 autoReconnect,reconnectDelayMs,
		 lwtQos,lwtTopic,lwtMessage) {
			var lwt = new Paho.MQTT.Message(lwtMessage);
				lwt.destinationName = lwtTopic;
				lwt.qos = lwtQos;
				lwt.retained = false;

			var instance = {
				client: new Paho.MQTT.Client(host, port, path, clientId),
				gameObjectName: gameObjectName,
				autoReconnect: autoReconnect,
				reconnectDelayMs: reconnectDelayMs,
				reconnectCount: -1,
				reconnectAttempts: 0,
				cachedClientId: clientId,
				connectOptions: {
					invocationContext: invocationContext,
					keepAliveInterval: keepAliveInterval,
					cleanSession: cleanSession,
					username: username,
					password: password,
					useSSL: useSSL,
					lwt: lwt
				}
			};

			//console.log(instance);

			var instanceId = IOTMQTT.instances.push(instance) - 1;
			IOTMQTT.hookClient(instanceId);
			return instanceId;
		},
		reconnect: function(instanceId) {
			CLOGGER(IOTMQTT.log,"reconnect()");
			var instance = IOTMQTT.instances[instanceId];
			return IOTMQTT.connect(instanceId);
		},
		connect: function(instanceId) {
			var instance = IOTMQTT.instances[instanceId];
			
			var options = {
				onSuccess: function(response) {
					response.__instanceId = instanceId;
					response.autoReconnect = instance.autoReconnect;
					response.clientId = instance.client.clientId;
					if(instance.autoReconnect) {
						instance.reconnectCount = instance.reconnectCount + 1;
						response.reconnectCount = instance.reconnectCount;
						response.reconnectAttempts = instance.reconnectAttempts;
						instance.reconnectAttempts = 0;

						if(instance.cachedClientId != instance.client.clientId) {
							instance.cachedClientId = instance.client.clientId;
							response.sessionLost = true;
						} else {
							response.sessionLost = false;
						}

						if(instance.reconnectCount > 0) {
							CLOGGER(IOTMQTT.log,'connect/onReconnect: ' + JSON.stringify(response));
							SendMessage(instance.gameObjectName,'HGIOTMQTTJS_onReconnect', JSON.stringify(response));
						}
					}
					CLOGGER(IOTMQTT.log,'connect/onSuccess: ' + JSON.stringify(response));
					SendMessage(instance.gameObjectName,'HGIOTMQTTJS_onConnectSuccess', JSON.stringify(response));
				},
				onFailure: function(response) {
					response.__instanceId = instanceId;
					response.autoReconnect = instance.autoReconnect;
					if(instance.autoReconnect) {
						instance.reconnectAttempts = instance.reconnectAttempts + 1;
						response.reconnectAttempts = instance.reconnectAttempts;
						setTimeout(function() { 
							IOTMQTT.recreateClient(instanceId);
							IOTMQTT.reconnect(instanceId); 
						}, instance.reconnectDelayMs);
					}
					CLOGGER(IOTMQTT.log,'connect/onFailure: ' + JSON.stringify(response));
					SendMessage(instance.gameObjectName,'HGIOTMQTTJS_onConnectFailure', JSON.stringify(response));
				}}//,
				//invocationContext: instance.connectOptions.invocationContext,
				//keepAliveInterval: instance.connectOptions.keepAliveInterval,
				//cleanSession: instance.connectOptions.cleanSession,
				//userName: instance.connectOptions.username,
				//password: instance.connectOptions.password,
				//useSSL: instance.connectOptions.useSSL,
				//willMessage: instance.connectOptions.lwt }
			
			try {
				instance.client.connect(options);
			} catch(e) {
				return false;
			}

			return true;
		}
	},
	Reconnect: function(instanceId) { 
		return IOTMQTT.reconnect(instanceId); 
	},
	Connect: function(gameObjectName, 
	 host, port, clientId, path, invocationContext,
	 keepAliveInterval, cleanSession, username, password, useSSL, 
	 autoReconnect,reconnectDelayMs,
	 lwtQos, lwtTopic, lwtMessage) {
	 	var __gameObjectName = Pointer_stringify(gameObjectName);
		var __host = Pointer_stringify(host);
		var __path = Pointer_stringify(path);
		var __clientId = Pointer_stringify(clientId);
		var __invocationContext = { "ic" : Pointer_stringify(invocationContext) };
		var __username = Pointer_stringify(username);
		var __password = Pointer_stringify(password);
		var __lwtTopic = Pointer_stringify(lwtTopic);
		var __lwtMessage = Pointer_stringify(lwtMessage);
		
		var instanceId = IOTMQTT.createClient(
			__gameObjectName,
			__host,
			port,
			__clientId,
			__path,
			keepAliveInterval,
			Boolean(cleanSession),
			__invocationContext,
			__username,
			__password,
			Boolean(useSSL),
			Boolean(autoReconnect),
			reconnectDelayMs,
			lwtQos,
			__lwtTopic,
			__lwtMessage);
		
		IOTMQTT.connect(instanceId);
		return instanceId;
	},
	Disconnect: function(instanceId) {
		var instance = IOTMQTT.instances[instanceId];

		try {
			instance.client.disconnect();
		} catch(e) {
			return false;
		}

		return true;
	},
	Send: function(instanceId, topic, message, qos, retained) {
		var __topic = Pointer_stringify(topic);
		var __message = Pointer_stringify(message);

		var instance = IOTMQTT.instances[instanceId];

		var mqttMessage = new Paho.MQTT.Message(__message);
		mqttMessage.destinationName = __topic;
		mqttMessage.qos = qos;
		mqttMessage.retained = Boolean(retained);

		try {
			instance.client.send(mqttMessage);
		} catch(e) {
			return false;
		}

		return true;
	},
	Subscribe: function(instanceId, filter, qos, invocationContext, timeout) {
		var __filter = Pointer_stringify(filter);
		var __invocationContext = { "ic" : Pointer_stringify(invocationContext) };

		var instance = IOTMQTT.instances[instanceId];

		try {
			instance.client.subscribe(__filter, {
				qos: qos,
				invocationContext: __invocationContext,
				timeout: timeout,
				onSuccess: function(response) {
					response.__instanceId = instanceId;
					CLOGGER(IOTMQTT.log,'subscribe/onSuccess: ' + JSON.stringify(response));
					SendMessage(instance.gameObjectName,'HGIOTMQTTJS_onSubscribeSuccess', JSON.stringify(response));
				},
				onFailure: function(response) {
					response.__instanceId = instanceId;
					CLOGGER(IOTMQTT.log,'subscribe/onFailure: ' + JSON.stringify(response));
					SendMessage(instance.gameObjectName,'HGIOTMQTTJS_onSubscribeFailure', JSON.stringify(response));
				}
			})
		} catch(e) {
			return false;
		}

		return true;
	},
	Unsubscribe: function(instanceId, filter, invocationContext, timeout) {
		var __filter = Pointer_stringify(filter);
		var __invocationContext = { "ic" : Pointer_stringify(invocationContext) };

		var instance = IOTMQTT.instances[instanceId];

		try {
			instance.client.unsubscribe(__filter, {
				invocationContext: __invocationContext,
				timeout: timeout,
				onSuccess: function(response) {
					response.__instanceId = instanceId;
					CLOGGER(IOTMQTT.log,'unsubscribe/onSuccess: ' + JSON.stringify(response));
					SendMessage(instance.gameObjectName,'HGIOTMQTTJS_onUnsubscribeSuccess', JSON.stringify(response));
				},
				onFailure: function(response) {
					response.__instanceId = instanceId;
					CLOGGER(IOTMQTT.log,'unsubscribe/onFailure: ' + JSON.stringify(response));
					SendMessage(instance.gameObjectName,'HGIOTMQTTJS_onUnsubscribeFailure', JSON.stringify(response));
				}
			})
		} catch(e) {
			return false;
		}

		return true;
	},
	StartTrace: function(instanceId) {
		var instance = IOTMQTT.instances[instanceId];
		instance.client.startTrace();
		return;
	},
	StopTrace: function(instanceId) {
		var instance = IOTMQTT.instances[instanceId];
		instance.client.stopTrace();
		return;
	},
	GetTraceLog: function(instanceId) {
		var instance = IOTMQTT.instances[instanceId];
		var traceLog = instance.client.getTraceLog();
		CLOGGER(IOTMQTT.log,JSON.stringify(traceLog));
		return null;	//TODO: return trace log
	}
};

autoAddDeps(HGIOTMQTTJS, '$IOTMQTT');
mergeInto(LibraryManager.library, HGIOTMQTTJS);