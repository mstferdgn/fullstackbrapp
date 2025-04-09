import React, { useState } from 'react';
import { Form, Input, Button, Card, message,Alert } from 'antd';
import API from '../services/api';
import { useNavigate, Link } from 'react-router-dom';
import axios from 'axios';
import api from '../services/api';


const Login = () => {
  const navigate = useNavigate();
  const [alertInfo, setAlertInfo] = useState({ visible: false, message: '', type: '' });
  const onFinish = async (values) => {
    // console.log(values)
    try {
      const res = await api.post('/account/login', values);
     
      
      localStorage.setItem('token', res.data.token);
      localStorage.setItem('userId', res.data.id);
      localStorage.setItem('userName',res.data.userName);
      
      setAlertInfo({ visible: true, message: "Giriş Başarılı", type: "success" });
      navigate('/books');
    } catch (err) {
        setAlertInfo({ visible: true, message: "Giriş Başarısız", type: "error" });
      console.error(err);
    }
  };
  return (
    <div style={{ maxWidth: 300, margin: 'auto', marginTop: '100px' }}>
    
    <Card title="Giriş Yap" style={{ width: 300, margin: 'auto', marginTop: '100px' }}>
      <Form onFinish={onFinish} layout="vertical">
        <Form.Item name="userName" label="Kullanıcı Adı">
          <Input />
        </Form.Item>
        <Form.Item name="password" label="Şifre">
          <Input.Password />
        </Form.Item>
        <Form.Item>
          <Button type="primary" htmlType="submit">Giriş Yap</Button>
          <Link to="/register" style={{ float: 'right' }}>Kayıt Ol</Link>
        </Form.Item>
      </Form>
    </Card>
    {alertInfo.visible && (
        <Alert 
          message={alertInfo.message} 
          type={alertInfo.type} 
          showIcon 
          closable 
          onClose={() => setAlertInfo({ ...alertInfo, visible: false })}
          style={{ marginBottom: '20px' }}
        />
      )}

    </div>
  );
};

export default Login;