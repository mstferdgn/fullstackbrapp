import React, { useEffect, useState } from 'react';
import { List, Button, Avatar, Modal, Form, Input, message,Alert } from 'antd';
import { UserOutlined, BookOutlined, PlusOutlined,SearchOutlined } from '@ant-design/icons';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import AuthorReviewPie from './AuthorReviewPie';
import api from '../services/api';

const Author = () => {
  const [authors, setAuthors] = useState([]);
  const [selectedAuthor, setSelectedAuthor] = useState(null);
  const [books, setBooks] = useState([]);
  const [modalVisible, setModalVisible] = useState(false);
  const [createVisible, setCreateVisible] = useState(false);
  const [alertInfo, setAlertInfo] = useState({ visible: false, message: '', type: '' });
  const [form] = Form.useForm();
  const navigate = useNavigate();

  const fetchAuthors = async () => {
    const res = await api.get('/authors');
    setAuthors(res.data);
  };


  const handleAuthorSearch = async (value) => {
    try {
      const res = await api.get(`/authors/search`, {
        params: { search: value }
      });
      setAuthors(res.data);
    } catch (error) {
      message.error('Yazarlar aranırken hata oluştu');
    }
  };

  const fetchBooksByAuthor = async (authorId) => {
    const res = await api.get(`/authors/${authorId}/books`);
    setBooks(res.data);
  };

  useEffect(() => {
    fetchAuthors();
  }, []);

  const handleAuthorClick = async (author) => {
    setSelectedAuthor(author);
    await fetchBooksByAuthor(author.id);
    setModalVisible(true);
  };

  const handleModalClose = () => {
    setModalVisible(false);
    setSelectedAuthor(null);
    setBooks([]);
  };

  const goToBookDetail = (bookId) => {
    navigate(`/books/${bookId}`);
  };

  const handleCreateAuthor = async (values) => {
    try {
      await api.post('/authors/create-author', values, {
        headers: { Authorization: `Bearer ${localStorage.getItem("token")}` }
      });
      setAlertInfo({ visible: true, message: 'Yazar başarıyla eklendi', type: 'success' });
      setCreateVisible(false);
      form.resetFields();
      fetchAuthors();
    } catch (err) {
      setAlertInfo({ visible: true, message: 'Bu yazar zaten sistemde mevcut', type: 'info' });
    }
  };

  return (
    <div style={{ maxWidth: '1100px', margin: '0 auto', padding: '20px' }}>
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
      <Input.Search
        placeholder="Yazar ara (Örn: ORHAN Pamuk)"
        enterButton={<SearchOutlined />}
        size="middle"
        onSearch={handleAuthorSearch}
        style={{ marginBottom: '20px', maxWidth: '300px' }}
      />

      {localStorage.getItem("token") && (
        <Button
          type="primary"
          icon={<PlusOutlined />}
          style={{ marginBottom: '20px' }}
          onClick={() => setCreateVisible(true)}
        >
          Yeni Yazar Ekle
        </Button>
      )}
  
      <div style={{ display: 'flex', gap: '32px', alignItems: 'flex-start' }}>
        {/* Sol: Yazar Listesi */}
        <div style={{ flex: 1 }}>
          <List
            itemLayout="horizontal"
            dataSource={authors}
            pagination={{ pageSize: 8, showSizeChanger: true }}
            renderItem={(author) => (
              <List.Item>
                <List.Item.Meta
                  avatar={<Avatar icon={<UserOutlined />} />}
                  title={
                    <Button type="link" onClick={() => handleAuthorClick(author)}>
                      {author.name}
                    </Button>
                  }
                />
              </List.Item>
            )}
          />
        </div>
  
        {/* Sağ: Pie Chart */}
        <div style={{ flex: 1 }}>
          <AuthorReviewPie />
        </div>
      </div>
  
      {/* Kitapları Listeleyen Modal */}
      <Modal
        open={modalVisible}
        title={`${selectedAuthor?.name} - Kitapları`}
        onCancel={handleModalClose}
        footer={null}
      >
        <List
          itemLayout="horizontal"
          dataSource={books}
          renderItem={(book) => (
            <List.Item onClick={() => goToBookDetail(book.id)} style={{ cursor: 'pointer' }}>
              <List.Item.Meta
                avatar={<Avatar shape="square" size={64} icon={<BookOutlined />} />}
                title={<Button type="link">{book.title}</Button>}
                description={book.description || 'Açıklama yok'}
              />
            </List.Item>
          )}
        />
      </Modal>
  
      {/* Yazar Ekleme Modalı */}
      <Modal
        open={createVisible}
        title="Yeni Yazar Ekle"
        onCancel={() => setCreateVisible(false)}
        footer={null}
      >
        <Form layout="vertical" form={form} onFinish={handleCreateAuthor}>
          <Form.Item
            name="name"
            label="Yazar Adı"
            rules={[{ required: true, message: 'Lütfen yazar adını giriniz!' }]}
          >
            <Input placeholder="Örn: Orhan Pamuk" />
          </Form.Item>
          <Button type="primary" htmlType="submit">Ekle</Button>
        </Form>
      </Modal>
    </div>
  );
}
export default Author;
