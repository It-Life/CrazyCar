package com.tastsong.crazycar.controller;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.annotation.Scope;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestHeader;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import com.tastsong.crazycar.Util.Util;
import com.tastsong.crazycar.common.Result;
import com.tastsong.crazycar.common.ResultCode;
import com.tastsong.crazycar.model.UserLoginRecordModel;
import com.tastsong.crazycar.model.UserModel;
import com.tastsong.crazycar.service.LoginService;

import cn.hutool.json.JSONObject;
import lombok.extern.slf4j.Slf4j;

@RestController
@Scope("prototype")
@RequestMapping(value = "/v1")
@Slf4j
public class LoginController {
	@Autowired
	private LoginService loginService;

	@PostMapping(value = "/Login")
	public Object login(@RequestBody JSONObject body) throws Exception {
		String userName = body.getStr("UserName")		;
		String password = body.getStr("Password");
		UserModel userModel = loginService.getUserByName(userName);
		log.info("login : userName = " + userName + "; password  = " + password);
		if (password.equals(userModel.user_password)){
			UserLoginRecordModel userLoginRecordModel = new UserLoginRecordModel();
			userLoginRecordModel.user_name = userName;
			userLoginRecordModel.login_time = System.currentTimeMillis()/1000;
			userLoginRecordModel.device = body.getStr("device");
			userLoginRecordModel.place = body.getStr("place");
			loginService.recordLoginInfo(userLoginRecordModel);
			return loginService.getUserInfo(userName);
		} else{
			return Result.failure(ResultCode.RC423);
		}
	}

	@GetMapping (value = "/TestJWT")
	public Object testJWT(@RequestHeader(Util.TOKEN) String token) throws Exception{
		log.info(Util.getUidByToken(token).toString());
		return Util.isLegalToken(token);
	}

	@PostMapping (value = "/Register")
	public Object register(@RequestBody JSONObject body) throws Exception{
		String userName = body.getStr("UserName");
		String password = body.getStr("Password");
		log.info("Register : UserName = " + userName + "; password  = " + password);
		if (loginService.isExistsUser(userName)){		
			return Result.failure(ResultCode.RC423);
		} else{
			loginService.registerUser(userName, password);
			if (loginService.isExistsUser(userName)){	
				return loginService.getUserInfo(userName);
			} else{
				return Result.failure(ResultCode.RC425);
			}
		}
	}
}
