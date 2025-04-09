// pages/BookReviews.js
import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import {
  Typography,
  List,
  Rate,
  message,
  Avatar,
  Collapse,
  Input,
  Button,
  Form,
  Modal,
  Alert
} from 'antd';
import axios from 'axios';
import { BookOutlined, PlusOutlined } from '@ant-design/icons';
import { DeleteOutlined, EditOutlined } from '@ant-design/icons';
import api from '../services/api';

const { Title } = Typography;
const { Panel } = Collapse;
const { TextArea } = Input;

const BookReviews = () => {
  const { bookId } = useParams();
  const [reviews, setReviews] = useState([]);
  const [alertInfo, setAlertInfo] = useState({ visible: false, message: '', type: '' });
  const [bookTitle, setBookTitle] = useState('');
  const [form] = Form.useForm();
  const [commentLoading, setCommentLoading] = useState(false);
  const [reviewModalVisible, setReviewModalVisible] = useState(false);
  const [reviewForm] = Form.useForm();
  const currentUserId = localStorage.getItem('userId');
  const [editingCommentId, setEditingCommentId] = useState(null);
  const [editingReviewId, setEditingReviewId] = useState(null);
  const [editedReviewText, setEditedReviewText] = useState('');
  const [editedRating, setEditedRating] = useState(0);


  useEffect(() => {
    const fetchReviews = async () => {
      try {
        const reviewRes = await api.get(`/reviews/${bookId}`);
        setReviews(reviewRes.data);
      } catch (err) {
        message.error('İncelemeler yüklenemedi');
      }
    };

    fetchReviews();
  }, [bookId]);

  const startEditing = (commentId) => {
    setEditingCommentId(commentId);
  };
  
  const cancelEditing = () => {
    setEditingCommentId(null);
  };
  
  const handleStartEditReview = (reviewId, currentText, currentRating) => {
    setEditingReviewId(reviewId);
    setEditedReviewText(currentText);
    setEditedRating(currentRating);
  };

  const cancelEditReview = () => {
    setEditingReviewId(null);
    setEditedReviewText('');
    setEditedRating(0);
  };

  const handleUpdateReview = async (reviewId) => {
    try {
      await api.put(`/reviews/update-review/${reviewId}`, {
        bookId:bookId,
        userId:currentUserId,
        reviewText: editedReviewText,
        rating: editedRating
      }, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('token')}`
        }
      });
      setAlertInfo({ visible: true, message: 'İnceleme güncellendi', type: 'success' });
      setEditingReviewId(null);
       const updated = await api.get(`/reviews/${bookId}`);
      setReviews(updated.data);
      setEditedRating(editedRating)
    } catch (error) {
        setAlertInfo({ visible: true, message: 'İnceleme güncellenemedi', type: 'error' });
    }
  }


  const handleDeleteReview = async (reviewId) => {
    try {
      await api.delete(`/reviews/delete-review/${reviewId}`, {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('token')}`
        }
      });
      setAlertInfo({ visible: true, message: 'İnceleme silindi', type: 'warning' });
      const updated = await api.get(`/reviews/${bookId}`);
      setReviews(updated.data);
    } catch (error) {
        setAlertInfo({ visible: true, message: 'İnceleme silinemedi', type: 'warning' });
    }
  };

  const handleDeleteComment = async (reviewId, commentId) => {
    await api.delete(`/comments/delete-comment/${commentId}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
    });
    
    // message.success({
    //     content: 'Yorum silidi',
    //     duration: 3 // saniye
    //   });
    setAlertInfo({ visible: true, message: 'Yorum silindi', type: 'success' });

    // tekrar fetch et
    const updated = await api.get(`/reviews/${bookId}`);;
      setReviews(updated.data);
  };
  
  const handleUpdateComment = async (commentId, updatedText, reviewId) => {
    await api.put(`/comments/edit-comment/${commentId}`, {
        id :commentId,
        commentText: updatedText,
        userId:currentUserId,
        reviewId:reviewId
    }, {
      headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
    });
    setAlertInfo({ visible: true, message: 'Yorum güncellendi', type: 'success' });
    setEditingCommentId(null);
    // tekrar fetch et
    const updated = await api.get(`/reviews/${bookId}`);
      setReviews(updated.data);
  };
  

  const handleAddComment = async (reviewId, values) => {
    try {
      setCommentLoading(true);
      let requestComment = {
        commentText : values.comment,
        userId : localStorage.getItem("userId"),
        reviewId : reviewId
      }
     const token = localStorage.getItem("token")
      await api.post(`/comments/create-comment`,requestComment,
        {
            headers: {
              Authorization: `Bearer ${token}`
            }
          }
      );
      setAlertInfo({ visible: true, message: 'Yorum eklendi', type: 'success' });
      
    
      // Yorumdan sonra tekrar fetch et
      const updated = await api.get(`/reviews/${bookId}`);
      setReviews(updated.data);
      form.resetFields();
    } catch (err) {
        setAlertInfo({ visible: true, message: 'Yorum eklenemedi', type: 'warning' });
    } finally {
      setCommentLoading(false);
    }
  };

  const handleCreateReview = async (values) => {
    try {
      await api.post(
        `/reviews/create-review`,
        {
          bookId: parseInt(bookId),
          userId: localStorage.getItem("userId"),
          reviewText: values.reviewText,
          rating: values.rating,
        },
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem('token')}`
          }
        }
      );
      setAlertInfo({ visible: true, message: 'İnceleme eklendi', type: 'success' });
      setReviewModalVisible(false);
      reviewForm.resetFields();
  
      const updated = await api.get(`/reviews/${bookId}`);
      setReviews(updated.data);
    } catch (err) {
        setAlertInfo({ visible: true, message: 'İnceleme eklenemedi', type: 'warning' });
    }
  };

  return (
    <div style={{ maxWidth: '700px', margin: '0 auto', padding: '20px' }}>
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
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Title level={3}>{reviews[0]?.bookTitle} - İncelemeleri</Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => setReviewModalVisible(true)}>
          Yeni İnceleme Yap
        </Button>
      </div>

      <List
        dataSource={reviews}
        renderItem={(review) => (
            
            <List.Item
            actions={
              review.userId === currentUserId
                ? [
                    <Button icon={<EditOutlined />} size="small" type="text" onClick={() => handleStartEditReview(review.id, review.reviewText, review.rating)} />,
                    <Button icon={<DeleteOutlined />} size="small" danger onClick={() => handleDeleteReview(review.id)} />
                  ]
                : []
            }
          >
            <List.Item.Meta
              avatar={<Avatar shape="square" size={64} icon={<BookOutlined />} />}
              title={
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Rate disabled value={review.rating} />
                  <span style={{ fontSize: '12px', color: '#999' }}>
                                    {new Date(review.createdAt? review.createdAt : review.updatedAt).toLocaleString('tr-TR')}
                                  </span>
                </div>
              }
              description={
                <>
                
                  <strong>{review?.createdBy}'ın İncelemesi:<br></br></strong>{' '}

                  {editingReviewId === review.id ? (
                    <div>
                      <TextArea
                        rows={3}
                        value={editedReviewText}
                        onChange={(e) => setEditedReviewText(e.target.value)}
                      />
                      <div style={{ marginTop: 8 }}>
                        <Rate
                          allowHalf
                          value={editedRating}
                          onChange={(value) => setEditedRating(value)}
                        />
                      </div>
                      <div style={{ marginTop: 8 }}>
                        <Button type="primary" size="small" onClick={() => handleUpdateReview(review.id)}>Kaydet</Button>
                        <Button size="small" onClick={cancelEditReview} style={{ marginLeft: 8 }}>Vazgeç</Button>
                      </div>
                    </div>
                  ) : (
                    review.reviewText
                  )}
                  <Collapse ghost>
                    <Panel header={`Yorumlar (${review.comments?.length || 0})`} key={`comments-${review.id}`}>
                      {review.comments && review.comments.length > 0 ? (
                        <List
                          itemLayout="horizontal"
                          dataSource={review.comments}
                          pagination={{
                            pageSize: 5,           
                      
                          }}
                          renderItem={(comment, index) => (
                            <List.Item
                            actions={
                                comment.userId === currentUserId
                                  ? [
                                      <Button
                                        danger
                                        size="small"
                                        icon={<DeleteOutlined />}
                                        onClick={() => handleDeleteComment(review.id, comment.id)}
                                      />,
                                      <Button
                                        type="text"
                                        size="small"
                                        icon={<EditOutlined />}
                                        onClick={() => startEditing(comment.id)}
                                      />
                                    ]
                                  : []
                              }
                          >
                            <List.Item.Meta
                              avatar={<Avatar src={`https://api.dicebear.com/7.x/miniavs/svg?seed=${index}`} />}
                              title={
                                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                                  <span>{comment.createdBy? comment.createdBy : comment.updatedBy}</span>
                                  <span style={{ fontSize: '12px', color: '#999' }}>
                                    {new Date(comment.createdAt? comment.createdAt : comment.updatedAt).toLocaleString('tr-TR')}
                                  </span>
                                </div>
                              }
                              description={
                                editingCommentId === comment.id ? (
                                  <Form onFinish={(values) => handleUpdateComment(comment.id, values.updatedText,review.id)}>
                                    <Form.Item name="updatedText" initialValue={comment.commentText} style={{ marginBottom: 0 }}>
                                      <TextArea rows={2} />
                                    </Form.Item>
                                    <Form.Item style={{ marginTop: 8 }}>
                                      <Button type="primary" htmlType="submit" size="small">
                                        Kaydet
                                      </Button>
                                      <Button onClick={() => cancelEditing()} size="small" style={{ marginLeft: 8 }}>
                                        Vazgeç
                                      </Button>
                                    </Form.Item>
                                  </Form>
                                ) : (
                                  comment.commentText
                                )
                              }
                            />
                          </List.Item>
                          )}
                        />
                      ) : (
                        <p>Henüz yorum yok.</p>
                      )}

                      <Form form={form} onFinish={(values) => handleAddComment(review.id, values)} layout="inline">
                        <Form.Item
                          name="comment"
                          rules={[{ required: true, message: 'Yorum boş olamaz!' }]}
                          style={{ flex: 1 }}
                        >
                          <TextArea rows={1} placeholder="Yorumunuzu yazın..." />
                        </Form.Item>
                        <Form.Item>
                          <Button type="primary" htmlType="submit" loading={commentLoading}>Gönder</Button>
                        </Form.Item>
                      </Form>
                    </Panel>
                  </Collapse>
                </>
              }
            />
          </List.Item>
        )}
      />

      <Modal
        open={reviewModalVisible}
        title="Yeni İnceleme Yap"
        onCancel={() => setReviewModalVisible(false)}
        footer={null}
      >
        <Form form={reviewForm} layout="vertical" onFinish={handleCreateReview}>
          <Form.Item
            name="reviewText"
            label="İnceleme"
            rules={[{ required: true, message: 'Lütfen inceleme yazınız!' }]}
          >
            <TextArea rows={3} placeholder="Bu kitap hakkında ne düşünüyorsunuz?" />
          </Form.Item>
          <Form.Item
            name="rating"
            label="Puan"
            rules={[{ required: true, message: 'Lütfen puan verin!' }]}
          >
            <Rate />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit">
              Gönder
            </Button>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default BookReviews;