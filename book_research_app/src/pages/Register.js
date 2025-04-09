import React, { useState } from "react";
import { Form, Input, Button, Card, message } from "antd";
import API from "../services/api";
import { useNavigate, Link } from "react-router-dom";
import axios from "axios";
import { Alert } from "antd";
import api from "../services/api";

const Register = () => {
  const navigate = useNavigate();
  const [successMessageVisible, setSuccessMessageVisible] = useState(false);
  const onFinish = async (values) => {
    // console.log(values);
    try {
      const res = await api.post(
        "/account/register",
        values
      );

      setSuccessMessageVisible(true); // mesajı göster
    

      message.success("Kayıt Başarılı!");
      
    } catch (err) {
      message.error("Kayıt Başarısız!");
      console.error(err);
    }
  };
  return (
    <Card
      title="Kayıt Ol"
      style={{ width: 300, margin: "auto", marginTop: "100px" }}
    >
      {successMessageVisible && (
        <Alert
          message="Kayıt Başarılı!"
          description="Kayıt başarıyla oluşturuldu. Lütfen mailinize gelen aktivasyon linkine tıklayarak hesabınızı aktif edin ve giriş yapın."
          type="success"
          showIcon
          style={{ marginBottom: "20px" }}
        />
      )}
      <Form onFinish={onFinish} layout="vertical">
        <Form.Item
          name="firstName"
          label="Adınız"
          rules={[
            { required: true, message: "Lütfen adınızı giriniz!" },
            { min: 2, message: "Adınız en az 2 karakter olmalı!" },
            { max: 30, message: "Adınız en fazla 30 karakter olabilir!" },
          ]}
        >
          <Input />
        </Form.Item>

        <Form.Item
          name="lastName"
          label="Soyadınız"
          rules={[
            { required: true, message: "Lütfen soyadınızı giriniz!" },
            { min: 2, message: "Soyadınız en az 2 karakter olmalı!" },
            { max: 30, message: "Soyadınız en fazla 30 karakter olabilir!" },
          ]}
        >
          <Input />
        </Form.Item>
        <Form.Item
          name="emailAddress"
          label="Email"
          rules={[
            {
              required: true,
              message: "Lütfen email adresinizi giriniz!",
            },
            {
              type: "email",
              message: "Geçerli bir email adresi giriniz!",
            },
          ]}
        >
          <Input />
        </Form.Item>
        <Form.Item
          name="userName"
          label="Kullanıcı Adı"
          rules={[
            { required: true, message: "Lütfen kullanıcı adınızı giriniz!" },
            { min: 3, message: "Kullanıcı adı en az 3 karakter olmalı!" },
            {
              max: 20,
              message: "Kullanıcı adı en fazla 20 karakter olabilir!",
            },
          ]}
        >
          <Input />
        </Form.Item>

        <Form.Item
          name="password"
          label="Şifre"
          rules={[
            { required: true, message: "Lütfen şifrenizi giriniz!" },
            { min: 6, message: "Şifre en az 6 karakter olmalı!" },
            { max: 20, message: "Şifre en fazla 20 karakter olabilir!" },
          ]}
        >
          <Input.Password />
        </Form.Item>

        <Form.Item>
          <Button type="primary" htmlType="submit">
            Kayıt Ol
          </Button>
          <Link to="/login" style={{ float: "right" }}>
            Giriş yap
          </Link>
        </Form.Item>
      </Form>
    </Card>
  );
};

export default Register;
